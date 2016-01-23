#region

using System;
using System.Drawing;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Implementation of Raw encoding.
    /// </summary>
    public sealed class RawRectangle : EncodedRectangle
    {
        private readonly int[] pixels;

        public RawRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            /*
            bytes = PixelGrabber.GrabPixels(bmp, PixelFormat.Format32bppArgb);
            for (int i = 0; i < pixels.Length; i++)
                framebuffer[i] = pixels[i];
             */
            if (bytes == null)
                bytes = PixelGrabber.GrabPixels(pixels, new Rectangle(0, 0, rectangle.Width, rectangle.Height),
                    framebuffer);
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.RawEncoding));
            rfb.Write(bytes);

            /*  Very slow, not practically usable
            for (int i = 0; i < framebuffer.pixels.Length; i++)
                pwriter.WritePixel(framebuffer[i]);
            */
        }
    }
}