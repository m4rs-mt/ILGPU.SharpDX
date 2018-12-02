// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: CPUDirectXAccelerator.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime.CPU;
using ILGPU.Util;
using SharpDX.Direct3D11;
using System;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ILGPU.SharpDX.CPU
{
    /// <summary>
    /// Represents a CPU accelerator for DX interop.
    /// </summary>
    sealed class CPUDirectXAccelerator : DirectXInteropAccelerator
    {
        #region Static

        /// <summary>
        /// Convert the given view flags into a compatible MapMode.
        /// </summary>
        /// <param name="flags">The flags to convert.</param>
        /// <returns>The converted map mode.</returns>
        internal static MapMode ConvertToMapMode(DirectXViewFlags flags)
        {
            switch (flags)
            {
                case DirectXViewFlags.None:
                    return MapMode.ReadWrite;
                case DirectXViewFlags.ReadOnly:
                    return MapMode.Read;
                case DirectXViewFlags.WriteDiscard:
                    return MapMode.WriteDiscard;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flags));
            }
        }

        #endregion

        #region Instance

        /// <summary>
        /// Constructs a new CPU DX-interop accelerator.
        /// </summary>
        /// <param name="accelerator">The target CPU accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        internal CPUDirectXAccelerator(CPUAccelerator accelerator, Device d3dDevice)
            : base(accelerator, d3dDevice)
        { }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the assigned CPU accelerator.
        /// </summary>
        public CPUAccelerator CPUAccelerator => Accelerator as CPUAccelerator;

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
            return new CPUDirectXBuffer<T>(
                CPUAccelerator,
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
            return new CPUDirectXTexture2D(
                CPUAccelerator,
                D3DDevice,
                texture,
                bufferFlags,
                viewFlags);
        }

        /// <summary cref="DirectXInteropAccelerator.CreateMapping(DeviceContext, DirectXBuffer[])"/>
        protected override DirectXBufferMapping CreateMapping(
            DeviceContext context,
            DirectXBuffer[] buffers)
        {
            return new DirectXBufferMapping(this, context, buffers);
        }

        /// <summary cref="DirectXInteropAccelerator.UnmapBuffers(DeviceContext, DirectXBuffer[])"/>
        internal protected override void UnmapBuffers(
            DeviceContext context,
            DirectXBuffer[] buffers)
        {
            // Do nothing...
        }

        #endregion

        #region IDisposable

        /// <summary cref="DisposeBase.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        { }

        #endregion
    }
}
