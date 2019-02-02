// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: CudaDirectXAccelerator.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime.Cuda;
using ILGPU.Util;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Resource = SharpDX.Direct3D11.Resource;

namespace ILGPU.SharpDX.Cuda
{
    /// <summary>
    /// Represents a Cuda accelerator for DX interop.
    /// </summary>
    sealed class CudaDirectXAccelerator : DirectXInteropAccelerator
    {
        #region Static

        /// <summary>
        /// Registers the resource with the given flags in the scope of the Cuda runtime system.
        /// </summary>
        /// <param name="resource">The resource to register.</param>
        /// <param name="viewFlags">The view flags to use.</param>
        /// <param name="cudaGraphicsResource">The resulting graphics resource.</param>
        internal static void RegisterResource(
            Resource resource,
            DirectXViewFlags viewFlags,
            out IntPtr cudaGraphicsResource)
        {
            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsD3D11RegisterResource(
                    out cudaGraphicsResource,
                    resource.NativePointer,
                    CudaGraphicsRegisterFlags.None));

            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsResourceSetMapFlags(
                    cudaGraphicsResource,
                    (CudaGraphicsMapFlags)viewFlags));
        }

        #endregion

        #region Instance

        /// <summary>
        /// Constructs a new Cuda DX-interop accelerator.
        /// </summary>
        /// <param name="accelerator">The target Cuda accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        internal CudaDirectXAccelerator(CudaAccelerator accelerator, Device d3dDevice)
            : base(accelerator, d3dDevice)
        { }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the assigned Cuda accelerator.
        /// </summary>
        public CudaAccelerator CudaAccelerator => Accelerator as CudaAccelerator;

        #endregion

        #region Methods

        /// <summary cref="DirectXInteropAccelerator.CreateBuffer{T}(Buffer, DirectXBufferFlags, DirectXViewFlags)"/>
        public override DirectXBuffer<T> CreateBuffer<T>(
            Buffer buffer,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            return new CudaDirectXBuffer<T>(
                CudaAccelerator,
                D3DDevice,
                buffer,
                bufferFlags,
                viewFlags);
        }

        /// <summary cref="DirectXInteropAccelerator.CreateTexture2D(Texture2D, DirectXBufferFlags, DirectXViewFlags)"/>
        public override DirectXTexture2D CreateTexture2D(
            Texture2D texture,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            return new CudaDirectXTexture2D(
                CudaAccelerator,
                D3DDevice,
                texture,
                bufferFlags,
                viewFlags);
        }

        /// <summary cref="DirectXInteropAccelerator.CreateMapping(DeviceContext, DirectXBuffer[])"/>
        protected override unsafe DirectXBufferMapping CreateMapping(
            DeviceContext context,
            DirectXBuffer[] buffers)
        {
            IntPtr* cudaResources = stackalloc IntPtr[buffers.Length];
            for (int i = 0, e = buffers.Length; i < e; ++i)
            {
                var cudaBuffer = buffers[i] as ICudaDirectXBuffer;
                Debug.Assert(cudaBuffer != null, "Invalid Cuda buffer");
                cudaResources[i] = cudaBuffer.CudaGraphicsResource;
            }
            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsMapResources(
                    buffers.Length,
                    cudaResources,
                    IntPtr.Zero));
            return new DirectXBufferMapping(this, context, buffers);
        }

        /// <summary cref="DirectXInteropAccelerator.UnmapBuffers(DeviceContext, DirectXBuffer[])"/>
        internal protected override unsafe void UnmapBuffers(
            DeviceContext context,
            DirectXBuffer[] buffers)
        {
            IntPtr* cudaResources = stackalloc IntPtr[buffers.Length];
            for (int i = 0, e = buffers.Length; i < e; ++i)
            {
                var cudaBuffer = buffers[i] as ICudaDirectXBuffer;
                Debug.Assert(cudaBuffer != null, "Invalid Cuda buffer");
                cudaResources[i] = cudaBuffer.CudaGraphicsResource;
            }
            CudaException.ThrowIfFailed(
                CudaNativeMethods.cuGraphicsUnmapResources(
                    buffers.Length,
                    cudaResources,
                    IntPtr.Zero));
        }

        #endregion

        #region IDisposable

        /// <summary cref="DisposeBase.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        { }

        #endregion
    }
}
