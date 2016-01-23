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
    public sealed class ZlibRectangle : EncodedRectangle
    {
        private readonly int[] pixels;

        public ZlibRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            if (bytes == null)
                bytes = PixelGrabber.GrabPixels(pixels, new Rectangle(0, 0, rectangle.Width, rectangle.Height),
                    framebuffer);
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.ZlibEncoding));
            Console.WriteLine("ZLib uncompressed bytes size: " + bytes.Length);

            //ZlibRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(bytes);
            rfb.ZlibWriter.Flush();
        }

        private int Adler32(byte[] arr)
        {
            const uint a32mod = 65521;
            uint s1 = 1, s2 = 0;
            foreach (var b in arr)
            {
                s1 = (s1 + b)%a32mod;
                s2 = (s2 + s1)%a32mod;
            }
            return unchecked(Convert.ToInt32((s2 << 16) + s1));
        }
    }
}