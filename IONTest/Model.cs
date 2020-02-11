using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IONTest
{
    public class Model
    {
        public string TestString;
        public int TestInt;
        public float TestFloat;
        public double TestDouble;
        public long TestLong;
        public short TestShort;
        public List<int> TestList;
        public float[] TestArray;
        public SubClass TestSubClass;
        public decimal TestDecimal;
        public int TestInt2;
        public byte[] TestByte;
        public SubClass[] TestSubClassArray;
        public TestEnum TestEnum;
        public bool TestBool;
        public ushort TestUShort;
        public uint TestUInt;
        public ulong TestULong;      
    }

    public class SubClass
    {
        public string SubclassTestString;
    }

    public enum TestEnum
    {
        Option1,
        Option2
    }
}
