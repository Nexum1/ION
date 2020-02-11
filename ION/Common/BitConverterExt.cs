using System;
using System.Collections.Generic;

namespace IONConvert
{
    class BitConverterExt
    {
        public static byte[] GetBytes(decimal dec)
        {
            Int32[] bits = decimal.GetBits(dec);
            List<byte> bytes = new List<byte>();
            foreach (Int32 i in bits)
            {
                bytes.AddRange(BitConverter.GetBytes(i));
            }
            return bytes.ToArray();
        }
        public static decimal ToDecimal(byte[] bytes, int index)
        {
            Int32[] bits = new Int32[4];
            for (int i = 0; i < 4; i++)
            {
                bits[i] = BitConverter.ToInt32(bytes, index);
                index += 4;
            }
            return new decimal(bits);
        }
    }
}