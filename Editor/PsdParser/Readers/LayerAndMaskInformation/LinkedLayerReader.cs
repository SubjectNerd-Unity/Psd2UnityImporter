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

using System;

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation
{
    class LinkedLayerReader : ValueReader<LinkedLayer>
    {
        public LinkedLayerReader(PsdReader reader)
            : base(reader, true, null)
        {
            
        }

        protected override long OnLengthGet(PsdReader reader)
        {
            return (reader.ReadInt64() + 3) & (~3);
        }

        protected override void ReadValue(PsdReader reader, object userData, out LinkedLayer value)
        {
            reader.ValidateSignature("liFD");
            int version = reader.ReadInt32();

            Guid id = new Guid(reader.ReadPascalString(1));
            string name = reader.ReadString();
            string type = reader.ReadType();
            string creator = reader.ReadType();
            long length = reader.ReadInt64();
            IProperties properties = reader.ReadBoolean() == true ? new DescriptorStructure(reader) : null;

            bool isDocument = this.IsDocument(reader);
            LinkedDocumentReader documentReader = null;
            LinkedDocumnetFileHeaderReader fileHeaderReader = null;
            if(length > 0 && isDocument == true)
            {
                long position = reader.Position;
                documentReader = new LinkedDocumentReader(reader, length);
                reader.Position = position;
                fileHeaderReader = new LinkedDocumnetFileHeaderReader(reader, length);
            }

            value = new LinkedLayer(name, id, documentReader, fileHeaderReader);
        }

        private bool IsDocument(PsdReader reader)
        {
            long position = reader.Position;
            try
            {
                return reader.ReadType() == "8BPS";
            }
            finally
            {
                reader.Position = position;
            }
        }
    }
}
