﻿namespace Veldrid.Graphics
{
    /// <summary>
    /// A <see cref="MaterialTextureInputElement"/> describing a <see cref="CubemapTexture"/>.
    /// </summary>
    public class CubemapTextureInputElement : MaterialTextureInputElement
    {
        private readonly ImageProcessorTexture _front;
        private readonly ImageProcessorTexture _back;
        private readonly ImageProcessorTexture _left;
        private readonly ImageProcessorTexture _right;
        private readonly ImageProcessorTexture _top;
        private readonly ImageProcessorTexture _bottom;

        /// <summary>
        /// Constructs a <see cref="CubemapTextureInputElement"/> from six face textures.
        /// </summary>
        /// <param name="name">The name of the cubemap texture.</param>
        /// <param name="front">The front texture.</param>
        /// <param name="back">The back texture.</param>
        /// <param name="left">The left texture.</param>
        /// <param name="right">The right texture.</param>
        /// <param name="top">The top texture.</param>
        /// <param name="bottom">The bottom texture.</param>
        public CubemapTextureInputElement(
            string name,
            ImageProcessorTexture front,
            ImageProcessorTexture back,
            ImageProcessorTexture left,
            ImageProcessorTexture right,
            ImageProcessorTexture top,
            ImageProcessorTexture bottom)
            : base(name)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            using (var frontPin = _front.Pixels.Pin())
            using (var backPin = _back.Pixels.Pin())
            using (var leftPin = _left.Pixels.Pin())
            using (var rightPin = _right.Pixels.Pin())
            using (var topPin = _top.Pixels.Pin())
            using (var bottomPin = _bottom.Pixels.Pin())
            {
                return rc.ResourceFactory.CreateCubemapTexture(
                    frontPin.Ptr,
                    backPin.Ptr,
                    leftPin.Ptr,
                    rightPin.Ptr,
                    topPin.Ptr,
                    bottomPin.Ptr,
                    _front.Width,
                    _front.Height,
                    RgbaFloat.SizeInBytes,
                    PixelFormat.R32_G32_B32_A32_Float);
            }
        }
    }
}
