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
using System.IO;
using SubjectNerd.PsdImporter.PsdParser.Readers;

namespace SubjectNerd.PsdImporter.PsdParser
{
    public class PsdDocument : IPsdLayer, IDisposable
    {
        private FileHeaderSectionReader fileHeaderSection;
        private ColorModeDataSectionReader colorModeDataSection;
        private ImageResourcesSectionReader imageResourcesSection;
        private LayerAndMaskInformationSectionReader layerAndMaskSection;
        private ImageDataSectionReader imageDataSection;

        private PsdReader reader;
        //private Uri baseUri;

        public PsdDocument()
        {
            
        }

        public static PsdDocument Create(string filename)
        {
            return PsdDocument.Create(filename, new PathResolver());
        }

        public static PsdDocument Create(string filename, PsdResolver resolver)
        {
            PsdDocument document = new PsdDocument();
            FileInfo fileInfo = new FileInfo(filename);
            FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            document.Read(stream, resolver, new Uri(fileInfo.DirectoryName));
            return document;
        }

        public static PsdDocument Create(Stream stream)
        {
            return PsdDocument.Create(stream, null);
        }

        public static PsdDocument Create(Stream stream, PsdResolver resolver)
        {
            PsdDocument document = new PsdDocument();
            document.Read(stream, resolver, new Uri(Directory.GetCurrentDirectory()));
            return document;
        }

        public void Dispose()
        {
            if (this.reader == null)
                return;

            this.reader.Dispose();
            this.reader = null;
            this.OnDisposed(EventArgs.Empty);
        }

        public FileHeaderSection FileHeaderSection
        {
            get { return this.fileHeaderSection.Value; }
        }

        public byte[] ColorModeData
        {
            get { return this.colorModeDataSection.Value; }
        }

        public int Width
        {
            get { return this.fileHeaderSection.Value.Width; }
        }

        public int Height
        {
            get { return this.fileHeaderSection.Value.Height; }
        }

        public int Depth
        {
            get { return this.fileHeaderSection.Value.Depth; }
        }

        public IPsdLayer[] Childs
        {
            get { return this.layerAndMaskSection.Value.Layers; }
        }

        public IEnumerable<ILinkedLayer> LinkedLayers
        {
            get { return this.layerAndMaskSection.Value.LinkedLayers; }
        }

        public IProperties Resources
        {
            get { return this.layerAndMaskSection.Value.Resources; }
        }

        public IProperties ImageResources
        {
            get { return this.imageResourcesSection; }
        }

        public bool HasImage
        {
            get
            {
                if (this.imageResourcesSection.Contains("Version") == false)
                    return false;
                return this.imageResourcesSection.ToBoolean("Version", "HasCompatibilityImage");
            }
        }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            if (this.Disposed != null)
            {
                this.Disposed(this, e);
            }
        }

        internal void Read(Stream stream, PsdResolver resolver, Uri uri)
        {
            this.reader = new PsdReader(stream, resolver, uri);
            this.reader.ReadDocumentHeader();

            this.fileHeaderSection = new FileHeaderSectionReader(this.reader);
            this.colorModeDataSection = new ColorModeDataSectionReader(this.reader);
            this.imageResourcesSection = new ImageResourcesSectionReader(this.reader);
            this.layerAndMaskSection = new LayerAndMaskInformationSectionReader(this.reader, this);
            this.imageDataSection = new ImageDataSectionReader(this.reader, this);
        }

        #region IPsdLayer

        IPsdLayer IPsdLayer.Parent
        {
            get { return null; }
        }

        bool IPsdLayer.IsClipping
        {
            get { return false; }
        }

        PsdDocument IPsdLayer.Document
        {
            get { return this; }
        }

        ILinkedLayer IPsdLayer.LinkedLayer
        {
            get { return null; }
        }

        string IPsdLayer.Name
        {
            get { return "Document"; }
        }

        int IPsdLayer.Left
        {
            get { return 0; }
        }

        int IPsdLayer.Top
        {
            get { return 0; }
        }

        int IPsdLayer.Right
        {
            get { return this.Width; }
        }

        int IPsdLayer.Bottom
        {
            get { return this.Height; }
        }

        BlendMode IPsdLayer.BlendMode
        {
            get { return BlendMode.Normal; }
        }

        IChannel[] IImageSource.Channels
        {
            get { return this.imageDataSection.Value; }
        }

        float IImageSource.Opacity
        {
            get { return 1.0f; }
        }

        bool IImageSource.HasMask
        {
            get { return this.FileHeaderSection.NumberOfChannels > 4; }
        }

        #endregion
    }
}
