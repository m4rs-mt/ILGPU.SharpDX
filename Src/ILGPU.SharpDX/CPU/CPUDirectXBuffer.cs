// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: CPUDirectXBuffer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ILGPU.SharpDX.CPU
{
    /// <summary>
    /// Represents a DX-compatible CPU buffer.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public sealed class CPUDirectXBuffer<T> : DirectXBuffer<T>, ICPUDirectXBuffer
        where T : struct
    {
        #region Instance

        private MemoryBuffer<T, Index> cpuMemory;
        private Buffer stagingBuffer;
        private DataBox box;

        /// <summary>
        /// Constructs a new CPU buffer for DX interop.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="buffer">The target DX buffer.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="viewFlags">The registration flags.</param>
        internal CPUDirectXBuffer(
            CPUAccelerator accelerator,
            Device d3dDevice,
            Buffer buffer,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator, d3dDevice, buffer, bufferFlags, viewFlags)
        {
            cpuMemory = Accelerator.Allocate<T, Index>(Length);

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = ElementSize * Length,
                StructureByteStride = ElementSize,
                Usage = ResourceUsage.Staging,
            };
            stagingBuffer = new Buffer(D3DDevice, desc);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the associated CPU accelerator.
        /// </summary>
        public CPUAccelerator CPUAccelerator => Accelerator as CPUAccelerator;

        #endregion

        #region Methods

        /// <summary cref="DirectXBuffer.OnMap(DeviceContext)"/>
        protected override unsafe IntPtr OnMap(DeviceContext context)
        {
            Debug.Assert(box.IsEmpty);
            var mapMode = CPUDirectXAccelerator.ConvertToMapMode(ViewFlags);

            // Copy the buffer to the staging buffer
            if (ViewFlags != DirectXViewFlags.WriteDiscard)
                context.CopyResource(Buffer, stagingBuffer);
            box = context.MapSubresource(stagingBuffer, 0, mapMode, MapFlags.None);

            // We have to copy the contents of the DX buffer into the CPU-memory buffer
            if (ViewFlags != DirectXViewFlags.WriteDiscard)
            {
                System.Buffer.MemoryCopy(
                    box.DataPointer.ToPointer(),
                    cpuMemory.NativePtr.ToPointer(),
                    cpuMemory.LengthInBytes,
                    cpuMemory.LengthInBytes);
            }
            return cpuMemory.NativePtr;
        }

        /// <summary cref="DirectXBuffer.OnUnmap(DeviceContext)"/>
        protected override unsafe void OnUnmap(DeviceContext context)
        {
            if (ViewFlags != DirectXViewFlags.ReadOnly)
            {
                // We have to copy the contents of the CPU-memory buffer into the DX buffer
                Debug.Assert(!box.IsEmpty);
                System.Buffer.MemoryCopy(
                    cpuMemory.NativePtr.ToPointer(),
                    box.DataPointer.ToPointer(),
                    cpuMemory.LengthInBytes,
                    cpuMemory.LengthInBytes);
            }

            context.UnmapSubresource(stagingBuffer, 0);
            box = default;

            if (ViewFlags != DirectXViewFlags.ReadOnly)
                context.CopyResource(stagingBuffer, Buffer);
        }

        #endregion

        #region IDisposable

        /// <summary cref="DirectXBuffer{T}.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Dispose(ref stagingBuffer);
            Dispose(ref cpuMemory);
        }

        #endregion
    }
}
