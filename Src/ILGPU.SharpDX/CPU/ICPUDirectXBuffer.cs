// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: ICPUDirectXBuffer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime.CPU;

namespace ILGPU.SharpDX.CPU
{
    /// <summary>
    /// Represents an abstract buffer for CPU-DX interop.
    /// </summary>
    interface ICPUDirectXBuffer
    {
        /// <summary>
        /// Returns the associated CPU accelerator.
        /// </summary>
        CPUAccelerator CPUAccelerator { get; }
    }
}
