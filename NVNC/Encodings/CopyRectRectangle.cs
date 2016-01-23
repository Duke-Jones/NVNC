#region

using System;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Implementation of CopyRect encoding.
    /// </summary>
    public sealed class CopyRectRectangle : EncodedRectangle
    {
        public CopyRectRectangle(VncHost rfb, Framebuffer framebuffer, Rectangle2 rectangle)
            : base(rfb, framebuffer, rectangle)
        {
        }

        // CopyRect Source Point (x,y) from which to copy pixels in Draw
        //Point source;

        public override void Encode()
        {
            //source = new Point();
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.X));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Y));
        }
    }
}