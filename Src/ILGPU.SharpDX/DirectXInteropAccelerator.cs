// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: DirectXInteropAccelerator.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Util;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents an abstract accelerator for DX interop.
    /// </summary>
    public abstract class DirectXInteropAccelerator : DisposeBase
    {
        #region Instance

        /// <summary>
        /// Constructs a new DX-interop accelerator.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        protected DirectXInteropAccelerator(Accelerator accelerator, Device d3dDevice)
        {
            Accelerator = accelerator ?? throw new ArgumentNullException(nameof(accelerator));
            D3DDevice = d3dDevice;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the associated accelerator.
        /// </summary>
        public Accelerator Accelerator { get; }

        /// <summary>
        /// Returns the type of the associated accelerator.
        /// </summary>
        public AcceleratorType AcceleratorType => Accelerator.AcceleratorType;

        /// <summary>
        /// Returns the associated DX device.
        /// </summary>
        public Device D3DDevice { get; }

        #endregion

        #region Methods

        #region Buffer

        /// <summary>
        /// Creates a DX buffer for interop.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="numElements">The number of elements.</param>
        /// <returns>The created DX buffer.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is required")]
        protected Buffer CreateDXBuffer<T>(int numElements)
            where T : struct
        {
            return CreateDXBuffer(numElements, DirectXBuffer<T>.ElementSize);
        }

        /// <summary>
        /// Creates a DX buffer for interop.
        /// </summary>
        /// <param name="numElements">The number of elements.</param>
        /// <param name="elementSize">The size of a single element.</param>
        /// <returns>The created DX buffer.</returns>
        protected Buffer CreateDXBuffer(int numElements, int elementSize)
        {
            if (numElements < 1)
                throw new ArgumentOutOfRangeException(nameof(numElements));
            if (elementSize < 1)
                throw new ArgumentOutOfRangeException(nameof(elementSize));
            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = elementSize * numElements,
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Default,
            };
            return new Buffer(D3DDevice, desc);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="numElements">The number of elements in the buffer.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public DirectXBuffer<T> CreateBuffer<T>(int numElements)
            where T : struct
        {
            return CreateBuffer<T>(numElements, DirectXBufferFlags.DisposeBuffer);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="numElements">The number of elements in the buffer.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public DirectXBuffer<T> CreateBuffer<T>(int numElements, DirectXBufferFlags bufferFlags)
            where T : struct
        {
            return CreateBuffer<T>(numElements, bufferFlags, DirectXViewFlags.None);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="numElements">The number of elements in the buffer.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <param name="viewFlags">The view flags to use.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public DirectXBuffer<T> CreateBuffer<T>(
            int numElements,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            where T : struct
        {
            return CreateBuffer<T>(
                CreateDXBuffer<T>(numElements),
                bufferFlags,
                viewFlags);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="buffer">The target DX buffer.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public DirectXBuffer<T> CreateBuffer<T>(Buffer buffer)
            where T : struct
        {
            return CreateBuffer<T>(buffer, DirectXBufferFlags.None);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="buffer">The target DX buffer.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public DirectXBuffer<T> CreateBuffer<T>(Buffer buffer, DirectXBufferFlags bufferFlags)
            where T : struct
        {
            return CreateBuffer<T>(buffer, bufferFlags, DirectXViewFlags.None);
        }

        /// <summary>
        /// Creates a DX-compatible buffer for processing.
        /// </summary>
        /// <typeparam name="T">The element type of the buffer.</typeparam>
        /// <param name="buffer">The target DX buffer.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <param name="viewFlags">The view flags to use.</param>
        /// <returns>The created DX-compatible buffer.</returns>
        public abstract DirectXBuffer<T> CreateBuffer<T>(
            Buffer buffer,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            where T : struct;

        #endregion

        #region Texture2D

        /// <summary>
        /// Creates a DX texture for interop.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <returns>The created DX texture.</returns>
        protected Texture2D CreateDXTexture2D(
            int width,
            int height,
            Format format)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            var desc = new Texture2DDescription()
            {
                ArraySize = 1,
                MipLevels = 1,
                Format = format,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                Width = width,
                Height = height
            };

            return new Texture2D(D3DDevice, desc);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public DirectXTexture2D CreateTexture2D(
            int width,
            int height,
            Format format)
        {
            return CreateTexture2D(
                width,
                height,
                format,
                DirectXBufferFlags.DisposeBuffer);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public DirectXTexture2D CreateTexture2D(
            int width,
            int height,
            Format format,
            DirectXBufferFlags bufferFlags)
        {
            return CreateTexture2D(
                width,
                height,
                format,
                bufferFlags,
                DirectXViewFlags.None);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <param name="viewFlags">The view flags to use.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public DirectXTexture2D CreateTexture2D(
            int width,
            int height,
            Format format,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
        {
            return CreateTexture2D(
                CreateDXTexture2D(width, height, format),
                bufferFlags,
                viewFlags);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="texture">The target DX texture.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public DirectXTexture2D CreateTexture2D(Texture2D texture)
        {
            return CreateTexture2D(
                texture,
                DirectXBufferFlags.None);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="texture">The target DX texture.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public DirectXTexture2D CreateTexture2D(
            Texture2D texture,
            DirectXBufferFlags bufferFlags)
        {
            return CreateTexture2D(
                texture,
                bufferFlags,
                DirectXViewFlags.None);
        }

        /// <summary>
        /// Creates a new DX-compatible 2D texture for processing.
        /// </summary>
        /// <param name="texture">The target DX texture.</param>
        /// <param name="bufferFlags">The buffer flags to use.</param>
        /// <param name="viewFlags">The view flags to use.</param>
        /// <returns>The created DX-compatible texture.</returns>
        public abstract DirectXTexture2D CreateTexture2D(
            Texture2D texture,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags);

        #endregion

        /// <summary>
        /// Maps the given buffers for processing.
        /// </summary>
        /// <param name="context">The device context to use.</param>
        /// <param name="buffers">The buffers to map.</param>
        /// <returns>A created buffer mapping.</returns>
        public DirectXBufferMapping MapBuffers(
            DeviceContext context,
            params DirectXBuffer[] buffers)
        {
#if DEBUG
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (buffers.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(buffers));
#endif
            return CreateMapping(context, buffers);
        }

        /// <summary>
        /// Maps the given buffers for processing.
        /// </summary>
        /// <param name="context">The device context to use.</param>
        /// <param name="buffers">The buffers to map.</param>
        /// <returns>A created buffer mapping.</returns>
        protected abstract DirectXBufferMapping CreateMapping(
            DeviceContext context,
            DirectXBuffer[] buffers);

        /// <summary>
        /// Unmaps the given mapped buffers.
        /// </summary>
        /// <param name="context">The device context to use.</param>
        /// <param name="buffers">The mapped buffers that have to be unmapped.</param>
        internal protected abstract void UnmapBuffers(
            DeviceContext context,
            DirectXBuffer[] buffers);

        #endregion
    }
}
