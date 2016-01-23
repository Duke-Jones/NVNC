#region

using System;
using System.IO;
using System.Text;

#endregion

namespace NVNC.Readers
{
    /// <summary>
    ///     BigEndianBinaryReader is a wrapper class used to read .NET integral types from a Big-Endian stream.  It inherits
    ///     from BinaryReader and adds Big- to Little-Endian conversion.
    /// </summary>
    public sealed class BigEndianBinaryReader : BinaryReader
    {
        private readonly byte[] buff = new byte[4];

        public BigEndianBinaryReader(Stream input)
            : base(input)
        {
        }

        public BigEndianBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        // Since this is being used to communicate with an RFB host, only some of the overrides are provided below.

        public override ushort ReadUInt16()
        {
            FillBuff(2);
            return (ushort) (buff[1] | (uint) buff[0] << 8);
        }

        public override short ReadInt16()
        {
            FillBuff(2);
            return (short) (buff[1] & 0xFF | buff[0] << 8);
        }

        public override uint ReadUInt32()
        {
            FillBuff(4);
            return (uint) buff[3] & 0xFF | (uint) buff[2] << 8 | (uint) buff[1] << 16 | (uint) buff[0] << 24;
        }

        public override int ReadInt32()
        {
            FillBuff(4);
            return Convert.ToInt32(buff[3] | buff[2] << 8 | buff[1] << 16 | buff[0] << 24);
        }

        private void FillBuff(int totalBytes)
        {
            var bytesRead = 0;
            var n = 0;
            do
            {
                n = BaseStream.Read(buff, bytesRead, totalBytes - bytesRead);
                if (n == 0)
                    throw new IOException("Unable to read next byte(s).");

                bytesRead += n;
            } while (bytesRead < totalBytes);
        }
    }
}