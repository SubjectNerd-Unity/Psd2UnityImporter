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
using System.Collections;
using System.Collections.Generic;

namespace SubjectNerd.PsdImporter.PsdParser
{
    class Properties : IProperties
    {
        private readonly Dictionary<string, object> props;

        public Properties()
        {
            this.props = new Dictionary<string, object>();
        }

        public Properties(int capacity)
        {
            this.props = new Dictionary<string, object>(capacity);
        }

        public void Add(string key, object value)
        {
            this.props.Add(key, value);
        }

        public bool Contains(string property)
        {
            string[] ss = property.Split(new char[] { '.', '[', ']', }, StringSplitOptions.RemoveEmptyEntries);

            object value = this.props;

            foreach (var item in ss)
            {
                if (value is ArrayList == true)
                {
                    ArrayList arrayList = value as ArrayList;
                    int index;
                    if (int.TryParse(item, out index) == false)
                        return false;
                    if (index >= arrayList.Count)
                        return false;
                    value = arrayList[index];
                }
                else if (value is IDictionary<string, object> == true)
                {
                    IDictionary<string, object> props = value as IDictionary<string, object>;
                    if (props.ContainsKey(item) == false)
                    {
                        return false;
                    }

                    value = props[item];
                }

            }
            return true; 
        }

        private object GetProperty(string property)
        {
            string[] ss = property.Split(new char[] { '.', '[', ']', }, StringSplitOptions.RemoveEmptyEntries);

            object value = this.props;

            foreach (var item in ss)
            {
                if (value is ArrayList == true)
                {
                    ArrayList arrayList = value as ArrayList;
                    value = arrayList[int.Parse(item)];
                }
                else if (value is IDictionary<string, object> == true)
                {
                    IDictionary<string, object> props = value as IDictionary<string, object>;
                    value = props[item];
                }
                else if (value is IProperties == true)
                {
                    IProperties props = value as IProperties;
                    value = props[item];
                }
            }
            return value;
        }

        public int Count
        {
            get { return this.props.Count; }
        }

        public object this[string property]
        {
            get
            {
                return this.GetProperty(property);
            }
            set
            {
                this.props[property] = value;
            }
        }

        #region IProperties

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this.props.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.props.GetEnumerator();
        }

        #endregion
    }
}
