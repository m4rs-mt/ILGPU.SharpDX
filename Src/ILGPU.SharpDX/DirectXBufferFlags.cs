// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
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
    /// Represents flags of a <see cref="DirectXBuffer"/>.
    /// </summary>
    [Flags]
    public enum DirectXBufferFlags
    {
        /// <summary>
        /// The default buffer flags. The referenced DX resource
        /// will not be disposed automatically.
        /// </summary>
        None = 0,

        /// <summary>
        /// The referenced DX resource will be disposed automatically.
        /// </summary>
        DisposeBuffer = 1,
    }
}
