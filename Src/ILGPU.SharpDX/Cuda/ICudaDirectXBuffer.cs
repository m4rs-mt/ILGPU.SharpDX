// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2017 Marcel Koester
//                                www.ilgpu.net
//
// File: ICudaDirectXBuffer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime.Cuda;
using System;

namespace ILGPU.SharpDX.Cuda
{
    /// <summary>
    /// Represents an abstract buffer for Cuda-DX interop.
    /// </summary>
    interface ICudaDirectXBuffer
    {
        /// <summary>
        /// Returns the associated Cuda accelerator.
        /// </summary>
        CudaAccelerator CudaAccelerator { get; }

        /// <summary>
        /// Returns the native handle to the associated Cuda graphics resource.
        /// </summary>
        IntPtr CudaGraphicsResource { get; }
    }
}
