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

#pragma warning disable 0219 // variable assigned but not used.
using System.Collections.Generic;

namespace SubjectNerd.PsdImporter.PsdParser.Structures
{
    class StructureObjectArray : Properties
    {
        public StructureObjectArray(PsdReader reader)
        {
            int version = reader.ReadInt32();
            this.Add("Name", reader.ReadString());
            this.Add("ClassID", reader.ReadKey());

            int count = reader.ReadInt32();

            List<Properties> items = new List<Properties>();

            for (int i = 0; i < count; i++)
            {
                Properties props = new Properties();
                props.Add("Type1", reader.ReadKey());
                props.Add("EnumName", reader.ReadType());


                props.Add("Type2", PsdUtility.ToUnitType(reader.ReadType()));
                int d4 = reader.ReadInt32();
                props.Add("Values", reader.ReadDoubles(d4));

                items.Add(props);
            }
            this.Add("items", items.ToArray());
        }
    }
}
