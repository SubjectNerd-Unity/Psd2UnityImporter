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

using System.Collections;

namespace SubjectNerd.PsdImporter.PsdParser.Structures
{
    class StructureEngineData : Properties
    {
        public StructureEngineData(PsdReader reader)
        {
            int length = reader.ReadInt32();
            reader.Skip('\n', 2);
            this.ReadProperties(reader, 0, this);
        }

        private void ReadProperties(PsdReader reader, int level, Properties props)
        {
            reader.Skip('\t', level);
            char c = reader.ReadChar();
            if (c == ']')
            {
                return;
            }
            else if (c == '<')
            {
                reader.Skip('<');
            }
            reader.Skip('\n');
            //Properties props = new Properties();
            while (true)
            {
                reader.Skip('\t', level);
                c = reader.ReadChar();
                if (c == '>')
                {
                    reader.Skip('>');
                    return;
                }
                else
                {
                    //assert c == 9;
                    c = reader.ReadChar();
                    //assert c == '/' : "unknown char: " + c + " on level: " + level;
                    string name = string.Empty;
                    while (true)
                    {
                        c = reader.ReadChar();
                        if (c == ' ' || c == 10)
                        {
                            break;
                        }
                        name += c;
                    }
                    if (c == 10)
                    {
                        Properties p = new Properties();
                        this.ReadProperties(reader, level + 1, p);
                        if (p.Count > 0)
                            props.Add(name, p);
                        reader.Skip('\n');
                    }
                    else if (c == ' ')
                    {
                        object value = this.ReadValue(reader, level + 1);
                        props.Add(name, value);
                    }
                    else
                    {
                        //assert false;
                    }
                }
            }
        }

        private object ReadValue(PsdReader reader, int level)
        {
            char c = reader.ReadChar();
            if (c == ']')
            {
                return null;
            }
            else if (c == '(')
            {
                // unicode string
                string text = string.Empty;
                int stringSignature = reader.ReadInt16() & 0xFFFF;
                //assert stringSignature == 0xFEFF;
                while (true)
                {
                    char b1 = reader.ReadChar();
                    if (b1 == ')')
                    {
                        reader.Skip('\n');
                        return text;
                    }
                    char b2 = reader.ReadChar();
                    if (b2 == '\\')
                    {
                        b2 = reader.ReadChar();
                    }
                    if (b2 == 13)
                    {
                        text += '\n';
                    }
                    else
                    {
                        text += (char)((b1 << 8) | b2);
                    }
                }
            }
            else if (c == '[')
            {
                ArrayList list = new ArrayList();
                // array
                c = reader.ReadChar();
                while (true)
                {
                    if (c == ' ')
                    {
                        object val = this.ReadValue(reader, level);
                        if (val == null)
                        {
                            reader.Skip('\n');
                            return list;
                        }
                        else
                        {
                            list.Add(val);
                        }
                    }
                    else if (c == 10)
                    {
                        Properties p = new Properties();
                        this.ReadProperties(reader, level, p);
                        reader.Skip('\n');
                        if (p.Count == 0)
                        {
                            return list;
                        }
                        else
                        {
                            list.Add(p);
                        }
                    }
                    else
                    {
                        //assert false;
                    }
                }
            }
            else
            {
                string value = string.Empty;
                do
                {
                    value += c;
                    c = reader.ReadChar();
                }
                while (c != 10 && c != ' ');

                {
                    int f;
                    if (int.TryParse(value, out f) == true)
                        return f;
                }
                {
                    float f;
                    if (float.TryParse(value, out f) == true)
                        return f;
                }
                {
                    bool f;
                    if (bool.TryParse(value, out f) == true)
                        return f;
                }

                return value;
            }
        }
    }
}
