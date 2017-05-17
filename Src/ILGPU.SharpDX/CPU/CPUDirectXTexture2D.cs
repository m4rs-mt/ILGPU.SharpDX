// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2017 Marcel Koester
//                                www.ilgpu.net
//
// File: CPUDirectXTexture2D.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;

namespace ILGPU.SharpDX.CPU
{
    /// <summary>
    /// Represents a DX-compatible CPU texture.
    /// </summary>
    public sealed class CPUDirectXTexture2D : DirectXTexture2D, ICPUDirectXBuffer
    {
        #region Instance

        private Texture2D stagingTexture;
        private MemoryBuffer<byte> cpuMemory;
        private DataBox box;

        /// <summary>
        /// Constructs a new CPU texture 2D.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="texture">The target DX texture.</param>
        /// <param name="bufferFlags">The used buffer flags.</param>
        /// <param name="viewFlags">The used view flags.</param>
        internal CPUDirectXTexture2D(
            CPUAccelerator accelerator,
            Device d3dDevice,
            Texture2D texture,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator, d3dDevice, texture, bufferFlags, viewFlags)
        {
            var desc = texture.Description;
            var stagingDesc = new Texture2DDescription()
            {
                ArraySize = 1,
                MipLevels = 1,
                Format = desc.Format,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new global::SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                Usage = ResourceUsage.Staging,
                Width = desc.Width,
                Height = desc.Height
            };

            stagingTexture = new Texture2D(d3dDevice, stagingDesc);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the associated CPU accelerator.
        /// </summary>
        public CPUAccelerator CPUAccelerator => Accelerator as CPUAccelerator;

        #endregion

        #region Methods

        /// <summary>
        /// Ensures that the CPU-memory buffer has the given size.
        /// </summary>
        /// <param name="size">The desired target size in bytes.</param>
        private void EnsureSpace(int size) 
        {
            if (cpuMemory == null || cpuMemory.Length != size)
            {
                cpuMemory?.Dispose();
                cpuMemory = Accelerator.Allocate<byte>(size);
            }
        }

        /// <summary cref="DirectXBuffer.OnMap(DeviceContext)"/>
        protected override unsafe IntPtr OnMap(DeviceContext context)
        {
            Debug.Assert(box.IsEmpty);

            // Copy the texture into the staging texture
            if (ViewFlags != DirectXViewFlags.WriteDiscard)
                context.CopyResource(Texture, stagingTexture);

            var mapMode = CPUDirectXAccelerator.ConvertToMapMode(ViewFlags);
            box = context.MapSubresource(stagingTexture, 0, mapMode, MapFlags.None);

            // Reserve enough space
            var lengthInBytes = box.SlicePitch;
            EnsureSpace(lengthInBytes);

            if (ViewFlags != DirectXViewFlags.WriteDiscard)
            {
                // Copy the contents of the staging texture into the CPU-memory buffer
                System.Buffer.MemoryCopy(
                    box.DataPointer.ToPointer(),
                    cpuMemory.Pointer.ToPointer(),
                    lengthInBytes,
                    lengthInBytes);
            }
            return cpuMemory.Pointer;
        }

        /// <summary cref="DirectXBuffer.OnUnmap(DeviceContext)"/>
        protected override unsafe void OnUnmap(DeviceContext context)
        {
            if (ViewFlags != DirectXViewFlags.ReadOnly)
            {
                // We have to copy the contents of the CPU-memory buffer into the DX buffer
                Debug.Assert(!box.IsEmpty);
                System.Buffer.MemoryCopy(
                    cpuMemory.Pointer.ToPointer(),
                    box.DataPointer.ToPointer(),
                    cpuMemory.LengthInBytes,
                    cpuMemory.LengthInBytes);
            }

            context.UnmapSubresource(stagingTexture, 0);
            box = default(DataBox);

            if (ViewFlags != DirectXViewFlags.ReadOnly)
                context.CopyResource(stagingTexture, Texture);
        }

        #endregion

        #region IDisposable

        /// <summary cref="DirectXBuffer{T}.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (stagingTexture == null)
                return;

            stagingTexture.Dispose();
            stagingTexture = null;

            cpuMemory?.Dispose();
            cpuMemory = null;
        }

        #endregion
    }
}
