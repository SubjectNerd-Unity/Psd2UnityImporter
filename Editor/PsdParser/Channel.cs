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

namespace SubjectNerd.PsdImporter.PsdParser
{
    public class Channel : IChannel
    {
        private byte[] data;
        private ChannelType type;
        private int height;
        private int width;
        private int[] rlePackLengths;
        private float opacity = 1.0f;
        private long size;

        public Channel(ChannelType type, int width, int height, long size)
        {
            this.type = type;
            this.width = width;
            this.height = height;
            this.size = size;
        }

        public Channel()
        {
            
        }

        public byte[] Data
        {
            get { return this.data; }
        }

        public ChannelType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public void ReadHeader(PsdReader reader, CompressionType compressionType)
        {
            if (compressionType != CompressionType.RLE)
                return;

            this.rlePackLengths = new int[this.height];
            if (reader.Version == 1)
            {
                for (int i = 0; i < this.height; i++)
                {
                    this.rlePackLengths[i] = reader.ReadInt16();
                }
            }
            else
            {
                for (int i = 0; i < this.height; i++)
                {
                    this.rlePackLengths[i] = reader.ReadInt32();
                }
            }
        }

        public void Read(PsdReader reader, int bpp, CompressionType compressionType)
        {
            switch (compressionType)
            {
                case CompressionType.Raw:
                    this.ReadData(reader, bpp, compressionType, null);
                    return;

                case CompressionType.RLE:
                    this.ReadData(reader, bpp, compressionType, this.rlePackLengths);
                    return;

                default:
                    break;
            }
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public float Opacity
        {
            get { return this.opacity; }
            set { this.opacity = value; }
        }

        public long Size
        {
            get { return this.size; }
            set { this.size = value; }
        }

        private void ReadData(PsdReader reader, int bps, CompressionType compressionType, int[] rlePackLengths)
        {
            int length = PsdUtility.DepthToPitch(bps, this.width);
            this.data = new byte[length * this.height];
            switch (compressionType)
            {
                case CompressionType.Raw:
                    reader.Read(this.data, 0, this.data.Length);
                    break;

                case CompressionType.RLE:
                    for (int i = 0; i < this.height; i++)
                    {
                        byte[] buffer = new byte[rlePackLengths[i]];
                        byte[] dst = new byte[length];
                        reader.Read(buffer, 0, rlePackLengths[i]);
                        DecodeRLE(buffer, dst, rlePackLengths[i], length);
                        for (int j = 0; j < length; j++)
                        {
                            this.data[(i * length) + j] = (byte)(dst[j] * this.opacity);
                        }
                    }
                    break;
            }
        }

        private static void DecodeRLE(byte[] src, byte[] dst, int packedLength, int unpackedLength)
        {
            int index = 0;
            int num2 = 0;
            int num3 = 0;
            byte num4 = 0;
            int num5 = unpackedLength;
            int num6 = packedLength;
            while ((num5 > 0) && (num6 > 0))
            {
                num3 = src[index++];
                num6--;
                if (num3 != 0x80)
                {
                    if (num3 > 0x80)
                    {
                        num3 -= 0x100;
                    }
                    if (num3 < 0)
                    {
                        num3 = 1 - num3;
                        if (num6 == 0)
                        {
                            throw new Exception("Input buffer exhausted in replicate");
                        }
                        if (num3 > num5)
                        {
                            throw new Exception(string.Format("Overrun in packbits replicate of {0} chars", num3 - num5));
                        }
                        num4 = src[index];
                        while (num3 > 0)
                        {
                            if (num5 == 0)
                            {
                                break;
                            }
                            dst[num2++] = num4;
                            num5--;
                            num3--;
                        }
                        if (num5 > 0)
                        {
                            index++;
                            num6--;
                        }
                        continue;
                    }
                    num3++;
                    while (num3 > 0)
                    {
                        if (num6 == 0)
                        {
                            throw new Exception("Input buffer exhausted in copy");
                        }
                        if (num5 == 0)
                        {
                            throw new Exception("Output buffer exhausted in copy");
                        }
                        dst[num2++] = src[index++];
                        num5--;
                        num6--;
                        num3--;
                    }
                }
            }
            if (num5 > 0)
            {
                for (num3 = 0; num3 < num6; num3++)
                {
                    dst[num2++] = 0;
                }
            }
        }
    }
}

