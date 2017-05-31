#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // variable assigned but never used.

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
using SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation;

namespace SubjectNerd.PsdImporter.PsdParser
{
    class LayerAndMaskInformationSection
    {
        private readonly LayerInfoReader layerInfo;
        private readonly GlobalLayerMaskInfoReader globalLayerMask;
        private readonly IProperties documentResources;

        private ILinkedLayer[] linkedLayers;

        public LayerAndMaskInformationSection(LayerInfoReader layerInfo, GlobalLayerMaskInfoReader globalLayerMask, IProperties documentResources)
        {
            this.layerInfo = layerInfo;
            this.globalLayerMask = globalLayerMask;
            this.documentResources = documentResources;
        }

        public PsdLayer[] Layers
        {
            get { return this.layerInfo.Value; }
        }

        public ILinkedLayer[] LinkedLayers
        {
            get
            {
                if (this.linkedLayers == null)
                {
                    List<ILinkedLayer> list = new List<ILinkedLayer>();
                    string[] ids = { "lnk2", "lnk3", "lnkD", "lnkE", };

                    foreach (var item in ids)
                    {
                        if (this.documentResources.Contains(item))
                        {
                            var items = this.documentResources.ToValue<ILinkedLayer[]>(item, "Items");
                            list.AddRange(items);
                        }
                    }
                    this.linkedLayers = list.ToArray();
                }
                return this.linkedLayers;
            }
        }

        public IProperties Resources
        {
            get { return this.documentResources; }
        }
    }
}
