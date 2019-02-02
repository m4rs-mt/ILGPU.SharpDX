// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: CudaDirectXBuffer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ILGPU.SharpDX.Cuda
{
    /// <summary>
    /// Represents a DX-compatible Cuda buffer.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public sealed class CudaDirectXBuffer<T> : DirectXBuffer<T>, ICudaDirectXBuffer
        where T : struct
    {
        #region Instance

        private IntPtr cudaGraphicsResource;

        /// <summary>
        /// Constructs a new Cuda buffer.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="buffer">The target DX buffer.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="viewFlags">The used view flags</param>
        internal CudaDirectXBuffer(
            CudaAccelerator accelerator,
            Device d3dDevice,
            Buffer buffer,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator, d3dDevice, buffer, bufferFlags, viewFlags)
        {
            CudaDirectXAccelerator.RegisterResource(
                Buffer,
                viewFlags,
                out cudaGraphicsResource);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the associated Cuda accelerator.
        /// </summary>
        public CudaAccelerator CudaAccelerator => Accelerator as CudaAccelerator;

        /// <summary>
        /// Returns the native handle to the associated Cuda graphics resource.
        /// </summary>
        public IntPtr CudaGraphicsResource => cudaGraphicsResource;

        #endregion

        #region Methods

        /// <summary cref="DirectXBuffer.OnMap(DeviceContext)"/>
        protected override unsafe IntPtr OnMap(DeviceContext context)
        {
            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsResourceGetMappedPointer(
                out IntPtr ptr,
                out UIntPtr size,
                cudaGraphicsResource));
            Debug.Assert(size.ToUInt32() >= Length * ElementSize);
            return ptr;
        }

        /// <summary cref="DirectXBuffer.OnUnmap(DeviceContext)"/>
        protected override unsafe void OnUnmap(DeviceContext context)
        {
            // We do not have to perform any special operation here...
        }

        #endregion

        #region IDisposable

        /// <summary cref="DirectXBuffer{T}.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (cudaGraphicsResource == IntPtr.Zero)
                return;

            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsUnregisterResource(
                    cudaGraphicsResource));
            cudaGraphicsResource = IntPtr.Zero;
        }

        #endregion
    }
}
