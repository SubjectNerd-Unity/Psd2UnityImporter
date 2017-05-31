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
    class LayerRecordsReader : ValueReader<LayerRecords>
    {
        private LayerRecordsReader(PsdReader reader)
            : base(reader, false, null)
        {
            
        }

        public static LayerRecords Read(PsdReader reader)
        {
            LayerRecordsReader instance = new LayerRecordsReader(reader);
            return instance.Value;
        }

        protected override void ReadValue(PsdReader reader, object userData, out LayerRecords value)
        {
            LayerRecords records = new LayerRecords();

            records.Top = reader.ReadInt32();
            records.Left = reader.ReadInt32();
            records.Bottom = reader.ReadInt32();
            records.Right = reader.ReadInt32();
            records.ValidateSize();

            int channelCount = reader.ReadUInt16();

            records.ChannelCount = channelCount;

            for (int i = 0; i < channelCount; i++)
            {
                records.Channels[i].Type = reader.ReadChannelType();
                records.Channels[i].Size = reader.ReadLength();
                records.Channels[i].Width = records.Width;
                records.Channels[i].Height = records.Height;
            }

            reader.ValidateSignature();

            records.BlendMode = reader.ReadBlendMode();
            records.Opacity = reader.ReadByte();
            records.Clipping = reader.ReadBoolean();
            records.Flags = reader.ReadLayerFlags();
            records.Filter = reader.ReadByte();

            value = records;
        }
    }
}
