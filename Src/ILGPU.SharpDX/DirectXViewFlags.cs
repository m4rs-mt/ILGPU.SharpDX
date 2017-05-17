// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2017 Marcel Koester
//                                www.ilgpu.net
//
// File: DirectXViewFlags.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using System;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents general flags for mapped buffers.
    /// </summary>
    [Flags]
    public enum DirectXViewFlags
    {
        /// <summary>
        /// The default flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The ILGPU-runtime cannot write to the buffer.
        /// </summary>
        ReadOnly = 1,

        /// <summary>
        /// The ILGPU-runtime cannot read from the buffer.
        /// </summary>
        WriteDiscard = 2
    }
}
