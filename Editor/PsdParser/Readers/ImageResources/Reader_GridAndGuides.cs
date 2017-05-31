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

using System.Collections.Generic;

namespace SubjectNerd.PsdImporter.PsdParser.Readers.ImageResources
{
    [ResourceID("1032", DisplayName = "GridAndGuides")]
    class Reader_GridAndGuides : ResourceReaderBase
    {
        public Reader_GridAndGuides(PsdReader reader, long length)
            : base(reader, length)
        {

        }

        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            Properties props = new Properties();

            int version = reader.ReadInt32();

            if (version != 1)
                throw new InvalidFormatException();

            props["HorizontalGrid"] = reader.ReadInt32();
            props["VerticalGrid"] = reader.ReadInt32();

            int guideCount = reader.ReadInt32();

            List<int> hg = new List<int>();
            List<int> vg = new List<int>();

            for (int i = 0; i < guideCount; i++)
            {
                int n = reader.ReadInt32();
                byte t = reader.ReadByte();

                if (t == 0)
                    vg.Add(n);
                else
                    hg.Add(n);
            }

            props["HorizontalGuides"] = hg.ToArray();
            props["VerticalGuides"] = vg.ToArray();

            value = props;
        }
    }
}
