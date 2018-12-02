// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: CudaDirectXTexture2D.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;

namespace ILGPU.SharpDX.Cuda
{
    /// <summary>
    /// Represents a DX-compatible Cuda texture.
    /// </summary>
    public sealed class CudaDirectXTexture2D : DirectXTexture2D, ICudaDirectXBuffer
    {
        #region Instance

        private MemoryBuffer<byte> buffer;
        private IntPtr cudaGraphicsResource;
        private IntPtr cudaArray;

        private CudaArrayDescriptor desc;
        private int pixelByteSize;

        /// <summary>
        /// Constructs a new Cuda texture 2D.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="texture">The target DX texture.</param>
        /// <param name="bufferFlags">The used buffer flags.</param>
        /// <param name="viewFlags">The used view flags.</param>
        internal CudaDirectXTexture2D(
            CudaAccelerator accelerator,
            Device d3dDevice,
            Texture2D texture,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator, d3dDevice, texture, bufferFlags, viewFlags)
        {
            CudaDirectXAccelerator.RegisterResource(
                texture,
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
            Debug.Assert(cudaArray == IntPtr.Zero);

            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsSubResourceGetMappedArray(
                    out cudaArray,
                    cudaGraphicsResource,
                    0,
                    0));

            Debug.Assert(cudaArray != IntPtr.Zero);

            if (buffer == null)
            {
                CudaException.ThrowIfFailed(
                    CudaNativeMethods.cuArrayGetDescriptor(out desc, cudaArray));

                pixelByteSize = CudaNativeMethods.GetByteSize(desc.arrayFormat) * desc.numChannels;
                buffer = Accelerator.Allocate<byte>(
                    desc.width.ToInt32() * desc.height.ToInt32() * pixelByteSize);
            }

            Debug.Assert(pixelByteSize > 0);

            if (ViewFlags != DirectXViewFlags.WriteDiscard)
            {
                // Copy texture data to buffer
                var args = new CudaMemcpy2DArgs()
                {
                    dstDevice = buffer.NativePtr,
                    dstMemoryType = CudaMemoryType.Device,

                    srcArray = cudaArray,
                    srcMemoryType = CudaMemoryType.Array,

                    WidthInBytes = new IntPtr(desc.width.ToInt32() * pixelByteSize),
                    Height = desc.height,
                };

                CudaException.ThrowIfFailed(
                    CudaNativeMethods.cuMemcpy2D(ref args));
            }

            return buffer.NativePtr;
        }

        /// <summary cref="DirectXBuffer.OnUnmap(DeviceContext)"/>
        protected override unsafe void OnUnmap(DeviceContext context)
        {
            Debug.Assert(pixelByteSize > 0);
            if (ViewFlags != DirectXViewFlags.ReadOnly)
            {
                // Copy buffer data to texture
                var args = new CudaMemcpy2DArgs()
                {
                    srcDevice = buffer.NativePtr,
                    srcMemoryType = CudaMemoryType.Device,

                    dstArray = cudaArray,
                    dstMemoryType = CudaMemoryType.Array,

                    WidthInBytes = new IntPtr(desc.width.ToInt32() * pixelByteSize),
                    Height = desc.height,
                };

                CudaException.ThrowIfFailed(
                    CudaNativeMethods.cuMemcpy2D(ref args));
            }
            cudaArray = IntPtr.Zero;
        }

        #endregion

        #region IDisposable

        /// <summary cref="DirectXBuffer{T}.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (cudaGraphicsResource == IntPtr.Zero)
                return;

            if (buffer != null)
                buffer.Dispose();
            buffer = null;

            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsUnregisterResource(
                    cudaGraphicsResource));
            cudaGraphicsResource = IntPtr.Zero;
        }

        #endregion
    }
}
