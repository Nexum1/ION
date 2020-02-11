using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IONConvert
{
    public static class ION
    {
        const BindingFlags serializationFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        const BindingFlags staticMethod = BindingFlags.Static | BindingFlags.NonPublic;

        #region Serialization
        public static string SerializeObjectString(object obj, bool structureParity = false, bool compression = false)
        {
            return Encoding.Unicode.GetString(SerializeObject(obj, structureParity, compression));
        }

        public static byte[] SerializeObject<T>(T obj, bool structureParity = false, bool compression = false)
        {
            BoolManager.Reset();
            int index = 0;
            List<byte> ion = ToION(obj, ref index);
            BoolManager.UpdateBools(ref ion);
            if (structureParity)
            {
                string parityStr = Parity(typeof(T));
                ion.InsertRange(0, parityStr.StringToByte());
            }
            byte[] ionBytes = ion.ToArray();
            if(compression)
            {
                ICompress compressor = new DefaultCompression();
                ionBytes = compressor.Compress(ionBytes);
            }
            return ionBytes;
        }

        static List<byte> ToION(object obj, ref int index)
        {
            List<byte> ion = new List<byte>();

            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields(serializationFlags);
            PropertyInfo[] properties = type.GetProperties(serializationFlags);

            foreach (FieldInfo field in fields)
            {
                var val = field.GetValue(obj);
                ion.AddRange(GetBytes(field.FieldType, field.GetValue(obj), ref index));
            }

            foreach (PropertyInfo property in properties)
            {
                var val = property.GetValue(obj);
                ion.AddRange(GetBytes(property.PropertyType, property.GetValue(obj), ref index));
            }

            return ion;
        }

        static byte[] GetBytes(Type type, object Object, ref int index)
        {
            string typeName = type.Name;
            Type arrayType = type;
            bool isArray = type.IsArray;

            if (Common.CustomIsPrimitive(type))//Primitives
            {
                return SerializationBitConverter(type, Object, typeName, ref index);
            }
            else if (isArray || Common.CustomIsList(type, typeName))//Arrays
            {
                if (!isArray)
                {
                    arrayType = type.GenericTypeArguments[0];
                }
                else
                {
                    arrayType = type.GetElementType();
                }
                typeName = arrayType.Name;

                return ArrayListSerialization(arrayType, typeName, Object, ref index);
            }
            else if (Common.CustomIsDictionary(type, typeName))//Dictionaries
            {
                Type key = type.GenericTypeArguments[0];
                Type value = type.GenericTypeArguments[1];

                throw new Exception("ION doesn't know how to serialize dictionaries");
            }
            else if (Common.CustomIsClass(type, typeName))
            {
                return SerializationBitConverter(type, Object, typeName, ref index);
            }
            else if (type.IsEnum)
            {
                return SerializationBitConverter(type, Object, typeName, ref index);
            }
            else
            {
                throw new Exception("ION doesn't know how to serialize base type " + typeName);
            }
        }

        static byte[] ArrayListSerialization(Type type, string typeName, object Object, ref int index)
        {
            List<byte> ion = new List<byte>();
            if (Object != null)
            {
                var arr = ((IEnumerable)Object).Cast<object>().ToArray();
                ion.AddRange(BitConverter.GetBytes(arr.Length));
                index += 4;
                foreach (var o in arr)
                {
                    ion.AddRange(SerializationBitConverter(type, o, typeName, ref index));
                }
            }
            else
            {
                ion.AddRange(BitConverter.GetBytes(-1));
                index += 4;
            }
            return ion.ToArray();
        }

        static byte[] SerializationBitConverter(Type Type, object Object, string TypeName, ref int index)
        {
            if (Common.CustomIsPrimitive(Type))
            {
                switch (TypeName)
                {
                    case "String":
                        List<byte> ion = new List<byte>();
                        if (Object != null)
                        {
                            string str = Object.ToString();
                            ion.AddRange(BitConverter.GetBytes(str.Length));
                            index += 4;
                            ion.AddRange(str.StringToByte());
                            index += str.Length;
                        }
                        else
                        {
                            ion.AddRange(BitConverter.GetBytes(0));
                            index += 4;
                        }
                        return ion.ToArray();
                    case "Boolean":
                        bool b = Convert.ToBoolean(Object);
                        BoolEditResult res = BoolManager.AddBool(b, ref index);
                        return new byte[0];
                    case "Byte":
                        index += 1;
                        return new byte[1] { (byte)Object };
                    case "Int16":
                        index += 2;
                        return BitConverter.GetBytes(Convert.ToInt16(Object));
                    case "UInt16":
                        index += 2;
                        return BitConverter.GetBytes(Convert.ToUInt16(Object));
                    case "Int32":
                        index += 4;
                        return BitConverter.GetBytes(Convert.ToInt32(Object));
                    case "UInt32":
                        index += 4;
                        return BitConverter.GetBytes(Convert.ToUInt32(Object));
                    case "Int64":
                        index += 8;
                        return BitConverter.GetBytes(Convert.ToInt64(Object));
                    case "UInt64":
                        index += 8;
                        return BitConverter.GetBytes(Convert.ToUInt64(Object));
                    case "Single":
                        index += 4;
                        return BitConverter.GetBytes(float.Parse(Object.ToString()));
                    case "Double":
                        index += 8;
                        return BitConverter.GetBytes(Convert.ToDouble(Object));
                    case "Decimal":
                        index += 16;
                        return BitConverterExt.GetBytes(Convert.ToDecimal(Object));
                    default:
                        throw new Exception("ION doesn't know how to serialize type " + TypeName);
                }
            }
            else if (Type.IsEnum)
            {
                byte enumByte = (byte)Convert.ChangeType(Object, typeof(byte));
                index += 1;
                return new byte[1] { enumByte };
            }
            else
            {
                List<byte> ion = new List<byte>();
                BoolEditResult res = new BoolEditResult();
                if (Object != null)
                {
                    res = BoolManager.AddBool(true, ref index);
                    ion.AddRange(ToION(Object, ref index));
                }
                else
                {
                    res = BoolManager.AddBool(false, ref index);
                }

                return ion.ToArray();
            }
        }
        #endregion

        #region Deserialization
        public static T DeserializeObject<T>(string ion, bool structureParity = false, bool compression = false)
        {
            return DeserializeObject<T>(Encoding.Unicode.GetBytes(ion), structureParity, compression);
        }

        public static T DeserializeObject<T>(byte[] ion, bool structureParity = false, bool compression = false)
        {
            if(compression)
            {
                ICompress compressor = new DefaultCompression();
                ion = compressor.Decompress(ion);
            }
            BoolManager.Reset();
            int index = 0;
            if (structureParity)
            {
                string parityStr = Parity(typeof(T));
                string sentParityStr = ion.ByteToString(0, 64);

                if (parityStr != sentParityStr)
                {
                    throw new ParityException();
                }
                index = 64;
            }
            return FromION<T>(ion, ref index);
        }

        private static T FromION<T>(byte[] ion, ref int index)
        {
            Type type = typeof(T);
            T obj = (T)Activator.CreateInstance(type);
            FieldInfo[] fields = type.GetFields(serializationFlags);
            PropertyInfo[] properties = type.GetProperties(serializationFlags);

            foreach (FieldInfo field in fields)
            {
                object val = GetValueFromBytes(field.FieldType, ion, ref index);
                field.SetValue(obj, val);
            }

            foreach (PropertyInfo property in properties)
            {
                object val = GetValueFromBytes(property.PropertyType, ion, ref index);
                property.SetValue(obj, val);
            }

            return obj;
        }

        static object GetValueFromBytes(Type type, byte[] ion, ref int index)
        {
            string typeName = type.Name;
            bool isArray = type.IsArray;
            Type arrayType = type;
            if (Common.CustomIsPrimitive(type))//Primitives
            {
                return DeserializationBitConverter(ion, type, ref index);
            }
            else if (isArray || Common.CustomIsList(type, typeName))//Arrays
            {
                if (!isArray)
                {
                    arrayType = type.GenericTypeArguments[0];
                }
                else
                {
                    arrayType = type.GetElementType();
                }
                typeName = arrayType.Name;

                MethodInfo method = typeof(ION).GetMethod("ArrayListDeserialization", staticMethod);
                MethodInfo generic = method.MakeGenericMethod(arrayType);
                object[] args = new object[] { isArray, arrayType, ion, index };
                object obj = generic.Invoke(null, args);
                index = (int)args[3];
                return obj;
            }
            else if (Common.CustomIsDictionary(type, typeName))//Dictionaries
            {
                Type key = type.GenericTypeArguments[0];
                Type value = type.GenericTypeArguments[1];

                throw new Exception("ION doesn't know how to deserialize dictionaries");
            }
            else if (Common.CustomIsClass(type, typeName))
            {
                return DeserializationBitConverter(ion, type, ref index);
            }
            else if (type.IsEnum)
            {
                return DeserializationBitConverter(ion, type, ref index);
            }
            else
            {
                throw new Exception("ION doesn't know how to deserialize base type " + typeName);
            }
        }

        static object ArrayListDeserialization<T>(bool IsArray, Type type, byte[] ion, ref int index)
        {
            int length = BitConverter.ToInt32(ion, index);
            index += 4;

            if (length > -1)
            {
                List<T> arr = new List<T>();
                for (int i = 0; i < length; i++)
                {
                    arr.Add((T)DeserializationBitConverter(ion, type, ref index));
                }
                return IsArray ? (object)arr.ToArray() : (object)arr;
            }
            else
            {
                return null;
            }
        }

        static object DeserializationBitConverter(byte[] ion, Type type, ref int index)
        {
            string typeName = type.Name;

            if (Common.CustomIsPrimitive(type))
            {
                switch (typeName)
                {
                    case "String":
                        {
                            int length = BitConverter.ToInt32(ion, index);
                            index += 4;
                            string obj = length > 0 ? ion.ByteToString(index, length): null;
                            index += length;
                            return obj;
                        }
                    case "Boolean":
                        {
                            return BoolManager.NextBool(ion[index], ref index);
                        }
                    case "Byte":
                        {
                            byte obj = ion[index];
                            index += 1;
                            return obj;
                        }
                    case "Int16":
                        {
                            short obj = BitConverter.ToInt16(ion, index);
                            index += 2;
                            return obj;
                        }
                    case "UInt16":
                        {
                            ushort obj = BitConverter.ToUInt16(ion, index);
                            index += 2;
                            return obj;
                        }
                    case "Int32":
                        {
                            int obj = BitConverter.ToInt32(ion, index);
                            index += 4;
                            return obj;
                        }
                    case "UInt32":
                        {
                            uint obj = BitConverter.ToUInt32(ion, index);
                            index += 4;
                            return obj;
                        }
                    case "Int64":
                        {
                            long obj = BitConverter.ToInt64(ion, index);
                            index += 8;
                            return obj;
                        }
                    case "UInt64":
                        {
                            ulong obj = BitConverter.ToUInt64(ion, index);
                            index += 8;
                            return obj;
                        }
                    case "Single":
                        {
                            float obj = BitConverter.ToSingle(ion, index);
                            index += 4;
                            return obj;
                        }
                    case "Double":
                        {
                            double obj = BitConverter.ToDouble(ion, index);
                            index += 8;
                            return obj;
                        }
                    case "Decimal":
                        {
                            decimal obj = BitConverterExt.ToDecimal(ion, index);
                            index += 16;
                            return obj;
                        }
                    default:
                        throw new Exception("ION doesn't know how to deserialize type " + typeName);
                }
            }
            else if (type.IsEnum)
            {
                object obj = Enum.Parse(type, ion[index].ToString());
                index += 1;
                return obj;
            }
            else
            {
                byte nextByte = index < ion.Length ? ion[index] : (byte)0;
                bool activeclass = BoolManager.NextBool(nextByte, ref index);
                if (activeclass)
                {
                    MethodInfo method = typeof(ION).GetMethod("FromION", staticMethod);
                    MethodInfo generic = method.MakeGenericMethod(type);
                    object[] args = new object[] { ion, index };
                    object obj = generic.Invoke(null, args);
                    index = (int)args[1];
                    return obj;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Parity
        public static string Parity(Type type)
        {
            return Parity(type, null);
        }

        public static string Parity(object data)
        {
            return Parity(data.GetType(), data);
        }

        public static string Parity(Type type, object data)
        {
            StringBuilder typeDetails = new StringBuilder();

            FieldInfo[] fields = type.GetFields(serializationFlags);
            PropertyInfo[] properties = type.GetProperties(serializationFlags);

            foreach (FieldInfo field in fields)
            {
                Type t = field.FieldType;
                string name = field.Name;
                string typename = t.Name;

                if (Common.CustomIsClass(t, typename))
                {
                    typeDetails.Append(t.Name + name);
                    if (data != null)
                    {
                        typeDetails.Append(Parity(field.FieldType, field.GetValue(data)));
                    }
                    else
                    {
                        typeDetails.Append(Parity(field.FieldType));
                    }
                }
                else
                {
                    if (data != null)
                    {
                        typeDetails.Append(t.Name + name + field.GetValue(data));
                    }
                    else
                    {
                        typeDetails.Append(t.Name + name);
                    }
                }
            }

            foreach (PropertyInfo property in properties)
            {
                Type t = property.PropertyType;
                string name = property.Name;
                string typename = t.Name;

                if (Common.CustomIsClass(t, typename))
                {
                    typeDetails.Append(t.Name + name);
                    if (data != null)
                    {
                        typeDetails.Append(Parity(property.PropertyType, property.GetValue(data)));
                    }
                    else
                    {
                        typeDetails.Append(Parity(property.PropertyType));
                    }
                }
                else
                {
                    if (data != null)
                    {
                        typeDetails.Append(t.Name + name + property.GetValue(data));
                    }
                    else
                    {
                        typeDetails.Append(t.Name + name);
                    }
                }
            }

            return Common.GetStringSha256Hash(typeDetails.ToString());
        }
        #endregion
    }
}