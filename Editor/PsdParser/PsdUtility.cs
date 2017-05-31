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
using System.Collections.Generic;

namespace SubjectNerd.PsdImporter.PsdParser
{
    static class PsdUtility
    {
        public static byte[] DecodeRLE(byte[] source)
        {
            List<byte> dest = new List<byte>();
            byte runLength;

            for (int i = 1; i < source.Length; i += 2)
            {
                runLength = source[i - 1];

                while (runLength > 0)
                {
                    dest.Add(source[i]);
                    runLength--;
                }
            }
            return dest.ToArray();
        }

        public static void DecodeRLE(byte[] src, byte[] dst, int packedLength, int unpackedLength)
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

        public static BlendMode ToBlendMode(string text)
        {
            switch (text.Trim())
            {
                case "pass":
                    return BlendMode.PassThrough;
                case "norm":
                    return BlendMode.Normal;
                case "diss":
                    return BlendMode.Dissolve;
                case "dark":
                    return BlendMode.Darken;
                case "mul":
                    return BlendMode.Multiply;
                case "idiv":
                    return BlendMode.ColorBurn;
                case "lbrn":
                    return BlendMode.LinearBurn;
                case "dkCl":
                    return BlendMode.DarkerColor;
                case "lite":
                    return BlendMode.Lighten;
                case "scrn":
                    return BlendMode.Screen;
                case "div":
                    return BlendMode.ColorDodge;
                case "lddg":
                    return BlendMode.LinearDodge;
                case "lgCl":
                    return BlendMode.LighterColor;
                case "over":
                    return BlendMode.Overlay;
                case "sLit":
                    return BlendMode.SoftLight;
                case "hLit":
                    return BlendMode.HardLight;
                case "vLit":
                    return BlendMode.VividLight;
                case "lLit":
                    return BlendMode.LinearLight;
                case "pLit":
                    return BlendMode.PinLight;
                case "hMix":
                    return BlendMode.HardMix;
                case "diff":
                    return BlendMode.Difference;
                case "smud":
                    return BlendMode.Exclusion;
                case "fsub":
                    return BlendMode.Subtract;
                case "fdiv":
                    return BlendMode.Divide;
                case "hue":
                    return BlendMode.Hue;
                case "sat":
                    return BlendMode.Saturation;
                case "colr":
                    return BlendMode.Color;
                case "lum":
                    return BlendMode.Luminosity;
            }
            return BlendMode.Normal;
        }

        public static UnitType ToUnitType(string text)
        {
            switch (text)
            {
                case "#Ang":
                    return UnitType.Angle;
                case "#Rsl":
                    return UnitType.Density;
                case "#Rlt":
                    return UnitType.Distance;
                case "#Nne":
                    return UnitType.None;
                case "#Prc":
                    return UnitType.Percent;
                case "#Pxl":
                    return UnitType.Pixels;
                case "#Pnt":
                    return UnitType.Points;
                case "#Mlm":
                    return UnitType.Millimeters;
                default:
                    throw new NotSupportedException();
            }
        }

        public static int DepthToPitch(int depth, int width)
        {
            switch (depth)
            {
                case 1:
                    return width;//NOT Sure
                case 8:
                    return width;
                case 16:
                    return width * 2;
            }
            throw new NotSupportedException();
        }
    }
}
