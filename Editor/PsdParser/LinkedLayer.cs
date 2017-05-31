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
using SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation;

namespace SubjectNerd.PsdImporter.PsdParser
{
    class LinkedLayer : ILinkedLayer
    {
        private readonly string name;
        private readonly Guid id;
        private readonly LinkedDocumentReader documentReader;
        private readonly LinkedDocumnetFileHeaderReader fileHeaderReader;

        public LinkedLayer(string name, Guid id, LinkedDocumentReader documentReader, LinkedDocumnetFileHeaderReader fileHeaderReader)
        {
            this.name = name;
            this.id = id;
            this.documentReader = documentReader;
            this.fileHeaderReader = fileHeaderReader;
        }

        public PsdDocument Document
        {
            get
            {
                if (this.documentReader == null)
                    return null;
                return this.documentReader.Value; 
            }
        }

        public Uri AbsoluteUri
        {
            get { return null; }
        }

        public bool HasDocument
        {
            get { return this.documentReader != null; }
        }

        public Guid ID
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public int Width
        {
            get { return this.fileHeaderReader.Value.Width; }
        }

        public int Height
        {
            get { return this.fileHeaderReader.Value.Height; }
        }
    }
}
