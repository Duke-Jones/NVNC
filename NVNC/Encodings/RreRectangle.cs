#region

using System;
using System.Collections.Generic;
using System.IO;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Implementation of RRE encoding.
    /// </summary>
    public class RreRectangle : EncodedRectangle
    {
        protected internal int bgpixel;
        protected int[] pixels;
        protected internal SubRect[] subrects;

        public RreRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
            this.pixels = pixels;
        }

        public override unsafe void Encode()
        {
            var x = 0; //rectangle.X;
            var y = 0; //rectangle.Y;
            var w = rectangle.Width;
            var h = rectangle.Height;

            SubRect subrect;
            var vector = new List<SubRect>();

            int currentPixel;
            int runningX, runningY;
            var firstX = 0;
            var secondX = 0;
            bgpixel = GetBackground(pixels, w, x, y, w, h);

            fixed (int* px = pixels)
            {
                for (var currentY = y; currentY < h; currentY++)
                {
                    var line = currentY*w;
                    for (var currentX = x; currentX < w; currentX++)
                    {
                        if (*(px + (line + currentX)) != bgpixel)
                        {
                            currentPixel = *(px + (line + currentX));
                            var firstY = currentY - 1;
                            var firstYflag = true;
                            for (runningY = currentY; runningY < h; runningY++)
                            {
                                var segment = runningY*w;
                                if (*(px + (segment + currentX)) != currentPixel)
                                    break;
                                runningX = currentX;
                                while ((runningX < w) && (*(px + (segment + runningX)) == currentPixel))
                                    runningX++;
                                runningX--;
                                if (runningY == currentY)
                                    secondX = firstX = runningX;
                                if (runningX < secondX)
                                    secondX = runningX;
                                if (firstYflag && (runningX >= firstX))
                                    firstY++;
                                else
                                    firstYflag = false;
                            }
                            var secondY = runningY - 1;

                            var firstW = firstX - currentX + 1;
                            var firstH = firstY - currentY + 1;
                            var secondW = secondX - currentX + 1;
                            var secondH = secondY - currentY + 1;

                            subrect = new SubRect();
                            subrect.pixel = currentPixel;
                            subrect.x = (ushort) currentX;
                            subrect.y = (ushort) currentY;

                            if (firstW*firstH > secondW*secondH)
                            {
                                subrect.w = (ushort) firstW;
                                subrect.h = (ushort) firstH;
                            }
                            else
                            {
                                subrect.w = (ushort) secondW;
                                subrect.h = (ushort) secondH;
                            }
                            vector.Add(subrect);

                            for (runningY = subrect.y; runningY < subrect.y + subrect.h; runningY++)
                                for (runningX = subrect.x; runningX < subrect.x + subrect.w; runningX++)
                                    *(px + (runningY*w + runningX)) = bgpixel;
                        }
                    }
                }
            }
            subrects = vector.ToArray();
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(VncHost.Encoding.RreEncoding));
            rfb.WriteUInt32(Convert.ToUInt32(subrects.Length));
            WritePixel32(bgpixel);

            using (var ms = new MemoryStream())
            {
                for (var i = 0; i < subrects.Length; i++)
                {
                    var data = PixelGrabber.GrabBytes(subrects[i].pixel, framebuffer);

                    //This is how BigEndianBinaryWriter writes short values :)
                    var x = Flip(BitConverter.GetBytes(subrects[i].x));
                    var y = Flip(BitConverter.GetBytes(subrects[i].y));
                    var w = Flip(BitConverter.GetBytes(subrects[i].w));
                    var h = Flip(BitConverter.GetBytes(subrects[i].h));

                    ms.Write(data, 0, data.Length);
                    ms.Write(x, 0, x.Length);
                    ms.Write(y, 0, y.Length);
                    ms.Write(w, 0, w.Length);
                    ms.Write(h, 0, h.Length);
                }
                rfb.Write(ms.ToArray());
            }
        }

        private byte[] Flip(byte[] b)
        {
            // Given an array of bytes, flip and write to underlying stream
            Array.Reverse(b);
            return b;
        }

        protected internal class SubRect
        {
            public ushort h;
            public int pixel;
            public ushort w;
            public ushort x;
            public ushort y;
        }
    }
}