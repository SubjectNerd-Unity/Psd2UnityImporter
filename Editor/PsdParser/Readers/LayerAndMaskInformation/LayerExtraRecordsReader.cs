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
    class LayerExtraRecordsReader : ValueReader<LayerRecords>
    {
        private LayerExtraRecordsReader(PsdReader reader, LayerRecords records)
            : base(reader, true, records)
        {

        }

        public static LayerRecords Read(PsdReader reader, LayerRecords records)
        {
            LayerExtraRecordsReader instance = new LayerExtraRecordsReader(reader, records);
            return instance.Value;
        }

        protected override long OnLengthGet(PsdReader reader)
        {
            return reader.ReadUInt32();
        }

        protected override void ReadValue(PsdReader reader, object userData, out LayerRecords value)
        {
            LayerRecords records = userData as LayerRecords;
            LayerMask mask = LayerMaskReader.Read(reader);
            LayerBlendingRanges blendingRanges = LayerBlendingRangesReader.Read(reader);
            string name = reader.ReadPascalString(4);
            IProperties resources = new LayerResourceReader(reader, this.EndPosition - reader.Position);

            records.SetExtraRecords(mask, blendingRanges, resources, name);

            value = records;
        }
    }
}
