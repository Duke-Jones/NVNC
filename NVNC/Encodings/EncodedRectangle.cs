#region

using System;
using NVNC.Utils;

#endregion

namespace NVNC.Encodings
{
    /// <summary>
    ///     Abstract class representing an Rectangle to be encoded and written.
    /// </summary>
    public abstract class EncodedRectangle
    {
        protected Framebuffer framebuffer;
        protected Rectangle2 rectangle;
        protected VncHost rfb;

        public EncodedRectangle(VncHost rfb, Framebuffer framebuffer, Rectangle2 rectangle)
        {
            this.rfb = rfb;
            this.framebuffer = framebuffer;
            this.rectangle = rectangle;
        }

        public byte[] bytes { get; protected set; }

        /// <summary>
        ///     Gets the rectangle that needs to be encoded.
        /// </summary>
        public Rectangle2 UpdateRectangle
        {
            get { return rectangle; }
        }

        /// <summary>
        ///     Encode the pixel data from the supplied rectangle and store it in the bytes array.
        /// </summary>
        public abstract void Encode();

        /// <summary>
        ///     Writes the generic rectangle data to the stream.
        ///     It's coordinates and size.
        /// </summary>
        public virtual void WriteData()
        {
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.X));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Y));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Width));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Height));
        }

        protected void WritePixel32(int px)
        {
            var b = 0;
            var data = new byte[4];

            data[b++] = (byte) (px & 0xFF);
            data[b++] = (byte) ((px >> 8) & 0xFF);
            data[b++] = (byte) ((px >> 16) & 0xFF);
            data[b] = (byte) ((px >> 24) & 0xFF);

            rfb.Write(data);
        }

        protected int GetBackground(int[] pixels, int scanline, int x, int y, int w, int h)
        {
            return pixels[y*scanline + x];
            /*
            int runningX, runningY, k;
            int[] counts = new int[256];

            int maxcount = 0;
            int maxclr = 0;

            if( framebuffer.BitsPerPixel == 16 )
                return pixels[0];
            else if( framebuffer.BitsPerPixel == 32 )
                return pixels[0];

            // For 8-bit
            return pixels[0];

            for( runningX = 0; runningX < 256; runningX++ )
                counts[runningX] = 0;

            for( runningY = 0; runningY < pixels.Length; runningY++ )
            {
                k = pixels[runningY];
                if( k >= counts.Length )
                {
                    return 0;
                }
                counts[k]++;
                if( counts[k] > maxcount )
                {
                    maxcount = counts[k];
                    maxclr = pixels[runningY];
                }
            }
            return maxclr;
            */
        }
    }
}