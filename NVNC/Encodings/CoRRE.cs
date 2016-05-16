#region

using System;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    public class CoRRE : RreRectangle
    {
        public CoRRE(VncHost rfb, Framebuffer framebuffer, int[] pixels, Rectangle2 rectangle)
            : base(rfb, framebuffer, pixels, rectangle)
        {
            this.pixels = pixels;
        }

        public override void WriteData()
        {
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.X));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Y));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Width));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Height));

            rfb.WriteUInt32(Convert.ToUInt32(VncHost.Encoding.CoRreEncoding));
            rfb.WriteUInt32(Convert.ToUInt32(subrects.Length));

            WritePixel32(bgpixel);
            for (var i = 0; i < subrects.Length; i++)
            {
                WritePixel32(subrects[i].pixel);
                rfb.WriteByte(Convert.ToByte(subrects[i].x));
                rfb.WriteByte(Convert.ToByte(subrects[i].y));
                rfb.WriteByte(Convert.ToByte(subrects[i].w));
                rfb.WriteByte(Convert.ToByte(subrects[i].h));
            }
        }
    }
}