// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: DirectXBuffer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Util;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Resource = SharpDX.Direct3D11.Resource;
using System.Diagnostics;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents an abstract interop buffer for DX interop.
    /// </summary>
    public abstract class DirectXBuffer: ArrayViewSource, IMemoryBuffer
    {
        #region Instance

        /// <summary>
        /// Constructs a new buffer for DX interop.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="numElements">The number of elements.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="viewFlags">The registration flags.</param>
        protected DirectXBuffer(
            Accelerator accelerator,
            Device d3dDevice,
            int numElements,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator)
        {
            if (numElements < 1)
                throw new ArgumentOutOfRangeException(nameof(numElements));
            D3DDevice = d3dDevice;
            Length = numElements;
            BufferFlags = bufferFlags;
            ViewFlags = viewFlags;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the associated device.
        /// </summary>
        public Device D3DDevice { get; }

        /// <summary>
        /// The underlying DX resource (a buffer or a texture, for instance).
        /// </summary>
        public abstract Resource Resource { get; }

        /// <summary>
        /// Returns the resource view of this buffer.
        /// </summary>
        public ShaderResourceView ResourceView { get; protected set; }

        /// <summary>
        /// Returns the unordered-access view of this buffer.
        /// </summary>
        public UnorderedAccessView UnorderedAccessView { get; protected set; }

        /// <summary>
        /// Returns the used view flags.
        /// </summary>
        public DirectXViewFlags ViewFlags { get; private set; }

        /// <summary>
        /// Returns the used buffer flags.
        /// </summary>
        public DirectXBufferFlags BufferFlags { get; private set; }

        /// <summary>
        /// Returns true iff the resource is currently mapped.
        /// </summary>
        public bool IsMapped => NativePtr != IntPtr.Zero;

        /// <summary>
        /// Returns the number of elements.
        /// </summary>
        public int Length { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Maps the current buffer.
        /// </summary>
        internal void MapBuffer(DeviceContext context)
        {
            Debug.Assert(!IsMapped, "Buffer is already mapped");
            NativePtr = OnMap(context);
        }

        /// <summary>
        /// Creates a new mapping for this buffer and returns the
        /// accessible pointer for data access.
        /// </summary>
        /// <returns>The available access pointer.</returns>
        protected abstract IntPtr OnMap(DeviceContext context);

        /// <summary>
        /// Unmaps the current buffer.
        /// </summary>
        internal void UnmapBuffer(DeviceContext context)
        {
            Debug.Assert(IsMapped, "Buffer is not mapped");
            OnUnmap(context);
            NativePtr = IntPtr.Zero;
        }

        /// <summary>
        /// Disposes a previously performed mapping.
        /// </summary>
        protected abstract void OnUnmap(DeviceContext context);

        /// <summary cref="ArrayViewSource.GetAsRawArray(AcceleratorStream, Index, Index)"/>
        protected override ArraySegment<byte> GetAsRawArray(
            AcceleratorStream stream,
            Index byteOffset,
            Index byteExtent) =>
            throw new NotSupportedException();

        /// <summary cref="IMemoryBuffer.GetAsRawArray(AcceleratorStream)"/>
        public byte[] GetAsRawArray(AcceleratorStream stream) =>
            throw new NotSupportedException();

        /// <summary cref="IMemoryBuffer.MemSetToZero(AcceleratorStream)"/>
        public void MemSetToZero(AcceleratorStream stream) =>
            throw new NotSupportedException();

        #endregion

        #region IDisposable

        /// <summary cref="DisposeBase.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            ResourceView?.Dispose();
            ResourceView = null;

            UnorderedAccessView?.Dispose();
            UnorderedAccessView = null;
        }

        #endregion
    }

    /// <summary>
    /// Represents an abstract interop buffer for elements of type T for DX interop.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public abstract class DirectXBuffer<T> : DirectXBuffer
        where T : struct
    {
        #region Constants

        /// <summary>
        /// Represents the native size of a single element.
        /// </summary>
        public static readonly int ElementSize = ArrayView<T>.ElementSize;

        #endregion

        #region Instance

        private Buffer buffer;

        /// <summary>
        /// Constructs a new buffer for DX interop.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="buffer">The target DX buffer.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="viewFlags">The registration flags.</param>
        protected DirectXBuffer(
            Accelerator accelerator,
            Device d3dDevice,
            Buffer buffer,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(
                  accelerator,
                  d3dDevice,
                  buffer.Description.SizeInBytes / buffer.Description.StructureByteStride,
                  bufferFlags,
                  viewFlags)
        {
            var desc = buffer.Description;
            this.buffer = buffer;

            if ((desc.BindFlags & BindFlags.ShaderResource) != 0)
            {
                var resourceViewDesc = new ShaderResourceViewDescription()
                {
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementCount = Length,
                        FirstElement = 0,
                    },
                    Format = Format.Unknown,
                    Dimension = ShaderResourceViewDimension.Buffer,
                };

                ResourceView = new ShaderResourceView(D3DDevice, buffer, resourceViewDesc);
            }

            if ((desc.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                var uavDesc = new UnorderedAccessViewDescription()
                {
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        ElementCount = Length,
                        FirstElement = 0,
                    },
                    Format = Format.Unknown,
                    Dimension = UnorderedAccessViewDimension.Buffer,
                };

                UnorderedAccessView = new UnorderedAccessView(D3DDevice, buffer, uavDesc);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the underlying DX buffer.
        /// </summary>
        public Buffer Buffer => buffer;

        /// <summary cref="DirectXBuffer.Resource"/>
        public override Resource Resource => buffer;

        /// <summary>
        /// Returns an ILGPU view for accessing this buffer in a mapped context.
        /// </summary>
        public ArrayView<T> View => new ArrayView<T>(this, 0, Length);

        #endregion

        #region Methods

        /// <summary>
        /// Copies data from the given array to the buffer.
        /// </summary>
        /// <param name="context">The device context to use.</param>
        /// <param name="data">The data to copy.</param>
        public void CopyFrom(DeviceContext context, T[] data)
        {
            context.UpdateSubresource(data, Buffer);
        }

        #endregion

        #region IDisposable

        /// <summary cref="DisposeBase.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if ((BufferFlags & DirectXBufferFlags.DisposeBuffer) != 0)
                Dispose(ref buffer);
        }

        #endregion
    }
}
