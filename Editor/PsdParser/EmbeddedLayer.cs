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

namespace SubjectNerd.PsdImporter.PsdParser
{
    class EmbeddedLayer : ILinkedLayer
    {
        private readonly Guid id;
        private readonly PsdResolver resolver;
        private readonly Uri absoluteUri;
        private PsdDocument document;
        private readonly int width;
        private readonly int height;
        
        public EmbeddedLayer(Guid id, PsdResolver resolver, Uri absoluteUri)
        {
            this.id = id;
            this.resolver = resolver;
            this.absoluteUri = absoluteUri;

            if (File.Exists(this.absoluteUri.LocalPath) == true)
            {
                var header = FileHeaderSection.FromFile(this.absoluteUri.LocalPath);
                this.width = header.Width;
                this.height = header.Height;
            }
        }

        public PsdDocument Document
        {
            get
            {
                if (this.document == null)
                {
                    this.document = this.resolver.GetDocument(this.absoluteUri);
                }
                return this.document;
            }
        }

        public Uri AbsoluteUri
        {
            get { return this.absoluteUri; }
        }

        public bool HasDocument
        {
            get { return File.Exists(this.absoluteUri.LocalPath); }
        }

        public Guid ID
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.absoluteUri.LocalPath; }
        }

        public int Width
        {
            get { return this.width; }
        }

        public int Height
        {
            get { return this.height; }
        }
    }
}
