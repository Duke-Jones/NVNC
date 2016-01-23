#region

using System;
using System.IO;

#endregion

namespace ComponentAce.Compression.Libs.zlib
{
    public class ZOutputStream : Stream
    {
        protected internal byte[] buf, buf1 = new byte[1];
        protected internal int bufsize = 4096;
        protected internal bool compress;
        protected internal int flush_Renamed_Field;

        private Stream out_Renamed;

        protected internal ZStream z = new ZStream();

        public ZOutputStream(Stream out_Renamed)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.inflateInit();
            compress = false;
        }

        public ZOutputStream(Stream out_Renamed, int level)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.deflateInit(level);
            compress = true;
        }

        public virtual int FlushMode
        {
            get { return flush_Renamed_Field; }

            set { flush_Renamed_Field = value; }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        public virtual long TotalIn
        {
            get { return z.total_in; }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        public virtual long TotalOut
        {
            get { return z.total_out; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanRead
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanSeek
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanWrite
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override long Length
        {
            get { return 0; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override long Position
        {
            get { return 0; }

            set { }
        }

        private void InitBlock()
        {
            flush_Renamed_Field = zlibConst.Z_NO_FLUSH;
            buf = new byte[bufsize];
        }

        public void WriteByte(int b)
        {
            buf1[0] = (byte) b;
            Write(buf1, 0, 1);
        }

        //UPGRADE_TODO: The differences in the Expected value  of parameters for method 'WriteByte'  may cause compilation errors.  'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092_3"'
        public override void WriteByte(byte b)
        {
            WriteByte(b);
        }

        public override void Write(byte[] b1, int off, int len)
        {
            if (len == 0)
                return;
            int err;
            var b = new byte[b1.Length];
            Array.Copy(b1, 0, b, 0, b1.Length);
            z.next_in = b;
            z.next_in_index = off;
            z.avail_in = len;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                    err = z.deflate(flush_Renamed_Field);
                else
                    err = z.inflate(flush_Renamed_Field);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                out_Renamed.Write(buf, 0, bufsize - z.avail_out);
            } while (z.avail_in > 0 || z.avail_out == 0);
        }

        public virtual void finish()
        {
            int err;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                {
                    err = z.deflate(zlibConst.Z_FINISH);
                }
                else
                {
                    err = z.inflate(zlibConst.Z_FINISH);
                }
                if (err != zlibConst.Z_STREAM_END && err != zlibConst.Z_OK)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (bufsize - z.avail_out > 0)
                {
                    out_Renamed.Write(buf, 0, bufsize - z.avail_out);
                }
            } while (z.avail_in > 0 || z.avail_out == 0);
            try
            {
                Flush();
            }
            catch
            {
            }
        }

        public virtual void end()
        {
            if (compress)
            {
                z.deflateEnd();
            }
            else
            {
                z.inflateEnd();
            }
            z.free();
            z = null;
        }

        public override void Close()
        {
            try
            {
                try
                {
                    finish();
                }
                catch
                {
                }
            }
            finally
            {
                end();
                out_Renamed.Close();
                out_Renamed = null;
            }
        }

        public override void Flush()
        {
            out_Renamed.Flush();
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override void SetLength(long value)
        {
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }
    }
}