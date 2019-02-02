// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: DirectXTexture2D.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// Represents a 2D texture with ILGPU interop.
    /// Note that this texture can also be used as render target.
    /// </summary>
    public abstract class DirectXTexture2D : DirectXBuffer
    {
        #region Static

        static int GetNumElements(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            var desc = texture.Description;
            return desc.Width * desc.Height;
        }

        #endregion

        #region Instance

        private Texture2D texture;
        private RenderTargetView targetView;

        /// <summary>
        /// Constructs a new texture 2D with interop capabilities.
        /// </summary>
        /// <param name="accelerator">The target accelerator.</param>
        /// <param name="d3dDevice">The target DX device.</param>
        /// <param name="texture">The width.</param>
        /// <param name="bufferFlags">The used buffer flags.</param>
        /// <param name="viewFlags">The used view flags.</param>
        protected DirectXTexture2D(
            Accelerator accelerator,
            Device d3dDevice,
            Texture2D texture,
            DirectXBufferFlags bufferFlags,
            DirectXViewFlags viewFlags)
            : base(accelerator, d3dDevice, GetNumElements(texture), bufferFlags, viewFlags)
        {
            var desc = texture.Description;
            Width = desc.Width;
            Height = desc.Height;
            Format = desc.Format;
            this.texture = texture;

            if ((desc.BindFlags & BindFlags.RenderTarget) != 0)
                targetView = new RenderTargetView(d3dDevice, texture);
            if ((desc.BindFlags & BindFlags.ShaderResource) != 0)
                ResourceView = new ShaderResourceView(d3dDevice, texture);
            if ((desc.BindFlags & BindFlags.UnorderedAccess) != 0)
                UnorderedAccessView = new UnorderedAccessView(d3dDevice, texture);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the DirectX texture.
        /// </summary>
        public Texture2D Texture => texture;

        /// <summary cref="DirectXBuffer.Resource"/>
        public override Resource Resource => texture;

        /// <summary>
        /// Returns the render-target view.
        /// </summary>
        public RenderTargetView RenderTargetView => targetView;

        /// <summary>
        /// Returns the width of the texture.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Returns the height of the texture.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Returns the size of the texture as Index2.
        /// </summary>
        public Index2 Size => new Index2(Width, Height);

        /// <summary>
        /// Returns the DX format of the interop texture.
        /// </summary>
        public Format Format { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a linear array view pointing to this texture.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The created array view pointing to this texture.</returns>
        public ArrayView<T> GetLinearView<T>()
            where T : struct =>
            new ArrayView<T>(this, Index.Zero, Length);

        /// <summary>
        /// Returns an array view pointing to this texture.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The created array view pointing to this texture.</returns>
        public ArrayView2D<T> GetView<T>()
            where T : struct =>
            GetLinearView<T>().As2DView(Width, Height);

        #endregion

        #region IDisposable

        /// <summary cref="DirectXBuffer.Dispose(bool)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "targetView", Justification = "Dispose method will be invoked by a helper method")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Dispose(ref targetView);
            if ((BufferFlags & DirectXBufferFlags.DisposeBuffer) != 0)
                Dispose(ref texture);
        }

        #endregion
    }
}
