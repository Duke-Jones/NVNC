#region

using System;
using System.Diagnostics;
using System.IO;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Implementation of ZRLE encoding, as well as drawing support. See RFB Protocol document v. 3.8 section 6.6.5.
    /// </summary>
    public sealed class ZrleRectangle : EncodedRectangle
    {
        private const int TILE_WIDTH = 64;
        private const int TILE_HEIGHT = 64;
        private readonly int[] pixels;

        public ZrleRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            var x = 0; //rectangle.X;
            var y = 0; //rectangle.Y;
            var w = rectangle.Width;
            var h = rectangle.Height;

            Trace.WriteLine("Landed at ZRLE start!");

            //int rawDataSize = w * h * (framebuffer.BitsPerPixel / 8);
            //byte[] data = new byte[rawDataSize];

            //Bitmap bmp = PixelGrabber.GrabImage(rectangle.Width, rectangle.Height, pixels);
            using (var ms = new MemoryStream())
            {
                for (var currentY = y; currentY < y + h; currentY += TILE_HEIGHT)
                {
                    var tileH = TILE_HEIGHT;
                    tileH = Math.Min(tileH, y + h - currentY);
                    for (var currentX = x; currentX < x + w; currentX += TILE_WIDTH)
                    {
                        var tileW = TILE_WIDTH;
                        tileW = Math.Min(tileW, x + w - currentX);

                        var subencoding = rectangle.IsSolidColor ? (byte) 1 : (byte) 0;
                        ms.WriteByte(subencoding);

                        if (subencoding == 0)
                        {
                            var pixelz = PixelGrabber.CopyPixels(pixels, w, currentX, currentY, tileW, tileH);
                            for (var i = 0; i < pixelz.Length; ++i)
                            {
                                var b = 0;

                                //The CPixel structure (Compressed Pixel) has 3 bytes, opposed to the normal pixel which has 4.
                                var pixel = pixelz[i];
                                var pbytes = new byte[3];

                                pbytes[b++] = (byte) (pixel & 0xFF);
                                pbytes[b++] = (byte) ((pixel >> 8) & 0xFF);
                                pbytes[b++] = (byte) ((pixel >> 16) & 0xFF);
                                //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                                ms.Write(pbytes, 0, pbytes.Length);
                            }
                        }
                        else
                        {
                            var b = 0;
                            var pixel = rectangle.SolidColor;
                            var pbytes = new byte[3];

                            pbytes[b++] = (byte) (pixel & 0xFF);
                            pbytes[b++] = (byte) ((pixel >> 8) & 0xFF);
                            pbytes[b++] = (byte) ((pixel >> 16) & 0xFF);
                            //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                            ms.Write(pbytes, 0, pbytes.Length);
                        }
                    }
                }
                var uncompressed = ms.ToArray();
                bytes = uncompressed;
            }
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(VncHost.Encoding.ZrleEncoding));

            //ZrleRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(bytes);
            rfb.ZlibWriter.Flush();
        }
    }
}