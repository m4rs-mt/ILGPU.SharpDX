// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: DirectXBufferMapping.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using System;
using SharpDX.Direct3D11;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents a set of mapped buffers.
    /// </summary>
    public struct DirectXBufferMapping : IEquatable<DirectXBufferMapping>, IDisposable
    {
        #region Instance

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DirectXBufferMapping(
            DirectXInteropAccelerator accelerator,
            DeviceContext context,
            DirectXBuffer[] buffers)
        {
            Accelerator = accelerator;
            Context = context;
            Buffers = buffers;

            foreach (var buffer in Buffers)
                buffer.MapBuffer(context);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the associated DX-interop accelerator.
        /// </summary>
        public DirectXInteropAccelerator Accelerator { get; }

        /// <summary>
        /// Returns the associated DirectX device context.
        /// </summary>
        public DeviceContext Context { get; }

        /// <summary>
        /// Returns the mapped buffers.
        /// </summary>
        internal DirectXBuffer[] Buffers { get; private set; }

        /// <summary>
        /// Unmaps the mapped buffers.
        /// </summary>
        public void Unmap()
        {
            if (Buffers == null)
                return;

            Accelerator.UnmapBuffers(Context, Buffers);

            foreach (var buffer in Buffers)
                buffer.UnmapBuffer(Context);

            Buffers = null;
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Returns true iff the given mapping is equal to the current mapping.
        /// </summary>
        /// <param name="other">The other mapping.</param>
        /// <returns>True, iff the given mapping is equal to the current mapping.</returns>
        public bool Equals(DirectXBufferMapping other)
        {
            return this == other;
        }

        #endregion

        #region Object

        /// <summary>
        /// Returns true iff the given object is equal to the current mapping.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True, iff the given object is equal to the current mapping.</returns>
        public override bool Equals(object obj)
        {
            if (obj is DirectXBufferMapping)
                return Equals((DirectXBufferMapping)obj);
            return false;
        }

        /// <summary>
        /// Returns the hash code of this mapping.
        /// </summary>
        /// <returns>The hash code of this mapping.</returns>
        public override int GetHashCode()
        {
            return Accelerator.GetHashCode() ^ Context.GetHashCode() ^ Buffers.Length;
        }

        /// <summary>
        /// Returns the string representation of this mapping.
        /// </summary>
        /// <returns>The string representation of this mapping.</returns>
        public override string ToString()
        {
            return $"Mapping [Length: {Buffers.Length}]";
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns true iff the first and second mapping are the same.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>True, iff the first and second mapping are the same.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DirectXBufferMapping first, DirectXBufferMapping second)
        {
            if (first.Accelerator != second.Accelerator ||
                first.Context != second.Context ||
                first.Buffers.Length != second.Buffers.Length)
                return false;

            for (int i = 0, e = first.Buffers.Length; i < e; ++i)
            {
                if (first.Buffers[i] != second.Buffers[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true iff the first and second mapping are not the same.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>True, iff the first and second mapping are not the same.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DirectXBufferMapping first, DirectXBufferMapping second)
        {
            return !(first == second);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Unmaps the mapped buffers.
        /// </summary>
        public void Dispose()
        {
            Unmap();
        }

        #endregion
    }
}
