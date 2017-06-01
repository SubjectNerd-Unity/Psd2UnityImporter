#region License
//Ntreev Photoshop Document Parser for .Net
//
//Released under the MIT License.
//
//Copyright (c) 2015 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Text;

namespace SubjectNerd.PsdImporter.PsdParser
{
    public class PsdReader : IDisposable
    {
        private readonly BinaryReader reader;
        private readonly PsdResolver resolver;
        private readonly Stream stream;
        private readonly Uri uri;

        private int version = 1;

        public PsdReader(Stream stream, PsdResolver resolver, Uri uri)
        {
            this.stream = stream;
            this.reader = new InternalBinaryReader(stream);
            this.resolver = resolver;
            this.uri = uri;
        }

        public void Dispose()
        {
            this.reader.Close();
        }

        public string ReadType()
        {
            return this.ReadAscii(4);
        }

        public string ReadAscii(int length)
        {
            return Encoding.ASCII.GetString(this.reader.ReadBytes(length));
        }

        public bool VerifySignature()
        {
            return this.VerifySignature(false);
        }

        public bool VerifySignature(bool check64bit)
        {
            string signature = this.ReadType();

            if (signature == "8BIM")
                return true;

            if (check64bit == true && signature == "8B64")
                return true;
            
            return false;
        }

        public void ValidateSignature(string signature)
        {
            string s = this.ReadType();
            if(s != signature)
                throw new InvalidFormatException();
        }

        public void ValidateSignature()
        {
            this.ValidateSignature(false);
        }

        public void ValidateSignature(bool check64bit)
        {
            if (this.VerifySignature(check64bit) == false)
                throw new InvalidFormatException();
        }

        public void ValidateDocumentSignature()
        {
            string signature = this.ReadType();

            if (signature != "8BPS")
                throw new InvalidFormatException();
        }

        private void ValidateValue<T>(T value, string name, Func<T> readFunc)
        {
            T v = readFunc();
            if (object.Equals(value, v) == false)
            {
                throw new InvalidFormatException("{0}의 값이 {1}이 아닙니다.", name, value);
            }
        }

        public void ValidateInt16(short value, string name)
        {
            this.ValidateValue<short>(value, name, () => this.ReadInt16());
        }

        public void ValidateInt32(int value, string name)
        {
            this.ValidateValue<int>(value, name, () => this.ReadInt32());
        }

        public void ValidateType(string value, string name)
        {
            this.ValidateValue<string>(value, name, () => this.ReadType());
        }

        public string ReadPascalString(int modLength)
        {
            byte count = this.reader.ReadByte();
            string text = string.Empty;
            if (count == 0)
            {
                Stream baseStream = this.reader.BaseStream;
                baseStream.Position += modLength - 1;
                return text;
            }
            byte[] bytes = this.reader.ReadBytes(count);
            text = Encoding.UTF8.GetString(bytes);
            for (int i = count + 1; (i % modLength) != 0; i++)
            {
                Stream stream2 = this.reader.BaseStream;
                stream2.Position += 1L;
            }
            return text;
        }

        public string ReadString()
        {
            int length = this.ReadInt32();
            if (length == 0)
                return string.Empty;

            byte[] bytes = this.ReadBytes(length * 2);
            for (int i = 0; i < length; i++)
            {
                int index = i * 2;
                byte b = bytes[index];
                bytes[index] = bytes[index + 1];
                bytes[index + 1] = b;
            }

            if (bytes[bytes.Length - 1] == 0 && bytes[bytes.Length - 2] == 0)
            {
                length--;
            }

            return Encoding.Unicode.GetString(bytes, 0, length * 2);
        }

        public string ReadKey()
        {
            int length = this.ReadInt32();
            length = (length > 0) ? length : 4;
            return this.ReadAscii(length);
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return this.reader.Read(buffer, index, count);
        }

        public byte ReadByte()
        {
            return this.reader.ReadByte();
        }

        public char ReadChar()
        {
            return (char)this.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return this.reader.ReadBytes(count);
        }

        public bool ReadBoolean()
        {
            return this.ReverseValue(this.reader.ReadBoolean());
        }

        public double ReadDouble()
        {
            return this.ReverseValue(this.reader.ReadDouble());
        }

        public double[] ReadDoubles(int count)
        {
            double[] values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = this.ReadDouble();
            }
            return values;
        }

        public short ReadInt16()
        {
            return this.ReverseValue(this.reader.ReadInt16());
        }

        public int ReadInt32()
        {
            return this.ReverseValue(this.reader.ReadInt32());
        }

        public long ReadInt64()
        {
            return this.ReverseValue(this.reader.ReadInt64());
        }

        public ushort ReadUInt16()
        {
            return this.ReverseValue(this.reader.ReadUInt16());
        }

        public uint ReadUInt32()
        {
            return this.ReverseValue(this.reader.ReadUInt32());
        }

        public ulong ReadUInt64()
        {
            return this.ReverseValue(this.reader.ReadUInt64());
        }

        public long ReadLength()
        {
            return this.version == 1 ? this.ReadInt32() : this.ReadInt64();
        }

        public void Skip(int count)
        {
            this.ReadBytes(count);
        }

        public void Skip(char c)
        {
            char ch = this.ReadChar();
            if (ch != c)
                throw new NotSupportedException();
        }

        public void Skip(char c, int count)
        {
            for (int i = 0; i < count; i++)
            {
                this.Skip(c);
            }
        }

        public ColorMode ReadColorMode()
        {
            return (ColorMode)this.ReadInt16();
        }

        public BlendMode ReadBlendMode()
        {
            return PsdUtility.ToBlendMode(this.ReadAscii(4));
        }

        public LayerFlags ReadLayerFlags()
        {
			return (LayerFlags)this.ReadByte();
        }

        public ChannelType ReadChannelType()
        {
            return (ChannelType)this.ReadInt16();
        }

        public CompressionType ReadCompressionType()
        {
            return (CompressionType)this.ReadInt16();
        }

        public void ReadDocumentHeader()
        {
            this.ValidateDocumentSignature();
            this.Version = this.ReadInt16();
            this.Skip(6);
        }

        public long Position
        {
            get { return this.reader.BaseStream.Position; }
            set { this.reader.BaseStream.Position = value; }
        }

        public long Length
        {
            get { return this.reader.BaseStream.Length; }
        }

        public int Version
        {
            get { return this.version; }
            set 
            {
                if (value != 1 && value != 2)
                    throw new InvalidFormatException();

                this.version = value; 
            }
        }

        public PsdResolver Resolver
        {
            get { return this.resolver; }
        }

        public Stream Stream
        {
            get { return this.stream; }
        }

        public Uri Uri
        {
            get { return this.uri; }
        }

        private bool ReverseValue(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToBoolean(bytes, 0);
        }

        private double ReverseValue(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        private short ReverseValue(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        private int ReverseValue(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private long ReverseValue(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        private ushort ReverseValue(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private uint ReverseValue(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private ulong ReverseValue(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        class InternalBinaryReader : BinaryReader
        {
            public InternalBinaryReader(Stream stream)
                : base(stream)
            {

            }
        }
    }
}
