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

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation
{
    class LayerMaskReader : ValueReader<LayerMask>
    {
        private LayerMaskReader(PsdReader reader)
            : base(reader, true, null)
        {

        }

        public static LayerMask Read(PsdReader reader)
        {
            LayerMaskReader instance = new LayerMaskReader(reader);
            return instance.Value;
        }

        protected override long OnLengthGet(PsdReader reader)
        {
            return reader.ReadInt32();
        }

        protected override void ReadValue(PsdReader reader, object userData, out LayerMask value)
        {
            LayerMask mask = new LayerMask();
            mask.Top = reader.ReadInt32();
            mask.Left = reader.ReadInt32();
            mask.Bottom = reader.ReadInt32();
            mask.Right = reader.ReadInt32();
            mask.Color = reader.ReadByte();
            mask.Flag = reader.ReadByte();

            value = mask;
        }
    }
}
