using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IONConvert
{
    internal static class Common
    {
        internal static bool CustomIsDictionary(Type type, string typeName)
        {
            return typeName == "Dictionary`2" && type.IsGenericType;
        }

        internal static bool CustomIsList(Type type, string typeName)
        {
            return typeName == "List`1" && type.IsGenericType;
        }

        internal static bool CustomIsClass(Type type, string typeName)
        {
            return type.IsClass && !type.IsArray && !CustomIsPrimitive(type) && !CustomIsDictionary(type, typeName) && !CustomIsList(type, typeName);
        }

        internal static bool CustomIsPrimitive(Type type)
        {
            return type == typeof(string) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(byte) || type == typeof(bool) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        internal static byte[] StringToByte(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        internal static string ByteToString(this byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }

        internal static string ByteToString(this byte[] b, int index, int count)
        {
            return Encoding.UTF8.GetString(b, index, count);
        }
    }
}
