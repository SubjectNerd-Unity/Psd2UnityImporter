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

namespace SubjectNerd.PsdImporter.PsdParser
{
    public static class IPropertiesExtension
    {
        public static bool Contains(this IProperties props, string property, params string[] properties)
        {
            return props.Contains(GeneratePropertyName(property, properties));
        }

        public static T ToValue<T>(this IProperties props, string property, params string[] properties)
        {
            return (T)props[GeneratePropertyName(property, properties)];
        }

        public static Guid ToGuid(this IProperties props, string property, params string[] properties)
        {
            return new Guid(props.ToString(property, properties));
        }

        public static string ToString(this IProperties props, string property, params string[] properties)
        {
            return ToValue<string>(props, property, properties);
        }

        public static byte ToByte(this IProperties props, string property, params string[] properties)
        {
            return ToValue<byte>(props, property, properties);
        }

        public static int ToInt32(this IProperties props, string property, params string[] properties)
        {
            return ToValue<int>(props, property, properties);
        }

        public static float ToSingle(this IProperties props, string property, params string[] properties)
        {
            return ToValue<float>(props, property, properties);
        }

        public static double ToDouble(this IProperties props, string property, params string[] properties)
        {
            return ToValue<double>(props, property, properties);
        }

        public static bool ToBoolean(this IProperties props, string property, params string[] properties)
        {
            return ToValue<bool>(props, property, properties);
        }

        public static bool TryGetValue<T>(this IProperties props, ref T value, string property, params string[] properties)
        {
            string propertyName = GeneratePropertyName(property, properties);
            if (props.Contains(propertyName) == false)
                return false;
            value = props.ToValue<T>(propertyName);
            return true;
        }

        private static string GeneratePropertyName(string property, params string[] properties)
        {
            if (properties.Length == 0)
                return property;

            return property + "." + string.Join(".", properties);
        }
    }
}
