#region

using System;
using System.Collections.Generic;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Implementation of CoRRE encoding.
    /// </summary>
    public sealed class CoRreRectangle : EncodedRectangle
    {
        private readonly int[] pixels;

        public CoRRE[] rects;

        public CoRreRectangle(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
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

            CoRRE rect;
            var vector = new List<CoRRE>();


            if ((w <= 0xFF) && (h <= 0xFF))
            {
                rect = new CoRRE(rfb, framebuffer, pixels, rectangle);
                rect.Encode();
                vector.Add(rect);
            }
            else
            {
                int currentW, currentH;
                for (var currentY = 0; currentY < h; currentY += 0xFF)
                {
                    for (var currentX = 0; currentX < w; currentX += 0xFF)
                    {
                        try
                        {
                            currentW = w - currentX;
                            currentH = h - currentY;

                            if (currentW > 0xFF)
                                currentW = 0xFF;
                            if (currentH > 0xFF)
                                currentH = 0xFF;
                            var rc = new Rectangle2(x + currentX, y + currentY, currentW, currentH);
                            rect = new CoRRE(rfb, framebuffer, pixels, rc);

                            //problem ... WHY ?
                            rect.Encode();
                            vector.Add(rect);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.ReadLine();
                        }
                    }
                }
            }

            rects = vector.ToArray();
            //count = rects.length;
        }

        public override void WriteData()
        {
            Console.WriteLine(rects.Length);
            foreach (var r in rects)
                r.WriteData();
        }
    }
}