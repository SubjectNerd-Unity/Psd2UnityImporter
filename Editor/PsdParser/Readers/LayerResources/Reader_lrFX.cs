#pragma warning disable 0219 // variable assigned but not used.

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

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerResources
{
    [ResourceID("lrFX")]
    class Reader_lrFX : ResourceReaderBase
    {
        public Reader_lrFX(PsdReader reader, long length)
            : base(reader, length)
        {

        }

        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            value = new Properties();

            short version = reader.ReadInt16();
            int count = reader.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                string _8bim = reader.ReadAscii(4);
                string effectType = reader.ReadAscii(4);
                int size = reader.ReadInt32();
                long p = reader.Position;

                switch (effectType)
                {
                    case "dsdw":
                        {
                            //ShadowInfo.Parse(reader);
                        }
                        break;
                    case "sofi":
                        {
                            //this.solidFillInfo = SolidFillInfo.Parse(reader);
                        }
                        break;
                }

                reader.Position = p + size;
            }
        }
    }
}
