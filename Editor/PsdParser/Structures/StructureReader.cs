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

namespace SubjectNerd.PsdImporter.PsdParser.Structures
{
    static class StructureReader
    {
        public static object Read(string ostype, PsdReader reader)
        {
            switch (ostype)
            {
                case "obj ":
                    return new StructureReference(reader);

                case "Objc":
                    return new DescriptorStructure(reader, false);

                case "VlLs":
                    return new StructureList(reader);

                case "doub":
                    return reader.ReadDouble();

                case "UntF":
                    return new StructureUnitFloat(reader);

                case "TEXT":
                    return reader.ReadString();

                case "enum":
                    return new StructureEnumerate(reader);

                case "long":
                    return reader.ReadInt32();

                case "bool":
                    return reader.ReadBoolean();

                case "GlbO":
                    return new DescriptorStructure(reader, false);

                case "type":
                    return new StructureClass(reader);

                case "GlbC":
                    return new StructureClass(reader);

                case "alis":
                    return new StructureAlias(reader);

                case "tdta":
                    return new StructureUnknownOSType("Cannot read RawData");

                case "prop":
                    return new StructureProperty(reader);

                case "Clss":
                    return new StructureClass(reader);

                case "Enmr":
                    return new StructureEnumerate(reader);

                case "rele":
                    return new StructureOffset(reader);

                case "Idnt":
                    return new StructureUnknownOSType("Cannot read Identifier");

                case "indx":
                    return new StructureUnknownOSType("Cannot read Index");

                case "name":
                    return new StructureUnknownOSType("Cannot read Name");

                case "ObAr":
                    return new StructureObjectArray(reader);

            }
            throw new NotSupportedException(ostype);
        }
    }
}
