#region

using System;
using System.IO;
using System.Text;

#endregion

namespace NVNC.Writers
{
    /// <summary>
    ///     BigEndianBinaryWriter is a wrapper class used to write .NET integral types in Big-Endian order to a stream.  It
    ///     inherits from BinaryWriter and adds Little-to-Big-Endian conversion.
    /// </summary>
    public sealed class BigEndianBinaryWriter : BinaryWriter
    {
        public BigEndianBinaryWriter(Stream input)
            : base(input)
        {
        }

        public BigEndianBinaryWriter(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public override void Write(ushort value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(short value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(uint value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(int value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(ulong value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        public override void Write(long value)
        {
            FlipAndWrite(BitConverter.GetBytes(value));
        }

        private void FlipAndWrite(byte[] b)
        {
            // Given an array of bytes, flip and write to underlying stream
            Array.Reverse(b);
            base.Write(b);
        }
    }
}