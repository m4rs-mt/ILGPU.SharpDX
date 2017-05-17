// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2017 Marcel Koester
//                                www.ilgpu.net
//
// File: AcceleratorExtensions.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.SharpDX.CPU;
using ILGPU.SharpDX.Cuda;
using SharpDX.Direct3D11;
using System;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents extension methods for ILGPU accelerators.
    /// </summary>
    public static class AcceleratorExtensions
    {
        private struct ExtensionProvider : IAcceleratorExtensionProvider<DirectXInteropAccelerator>
        {
            public ExtensionProvider(Device d3dDevice)
            {
                D3DDevice = d3dDevice;
            }

            public Device D3DDevice { get; }

            public DirectXInteropAccelerator CreateCPUExtension(CPUAccelerator accelerator)
            {
                return new CPUDirectXAccelerator(accelerator, D3DDevice);
            }

            public DirectXInteropAccelerator CreateCudaExtension(CudaAccelerator accelerator)
            {
                return new CudaDirectXAccelerator(accelerator, D3DDevice);
            }
        }

        /// <summary>
        /// Creates a new DX-interop accelerator.
        /// </summary>
        /// <param name="accelerator">The source accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <returns>The created DX-interop accelerator.</returns>
        public static DirectXInteropAccelerator CreateDirectXInteropAccelerator(
            this Accelerator accelerator,
            Device d3dDevice)
        {
            if (d3dDevice == null)
                throw new ArgumentNullException(nameof(d3dDevice));
            return accelerator.CreateExtension<DirectXInteropAccelerator, ExtensionProvider>(
                new ExtensionProvider(d3dDevice));
        }
    }
}
