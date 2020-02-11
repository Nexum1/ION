using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ION
{
    class Program
    {
        static void Main(string[] args)
        {
            //PREP
            Model m = new Model();
            PopulateTestModel(ref m);

            //ION - SERIALIZE
            Stopwatch sw = Stopwatch.StartNew();
            byte[] ion = ION.SerializeObject(m);
            sw.Stop();
            long ionSerializeTime = sw.ElapsedTicks;

            //JSON - SERIALIZE
            sw = Stopwatch.StartNew();
            string jsonStr = JsonConvert.SerializeObject(m);
            sw.Stop();
            long jsonSerializeTime = sw.ElapsedTicks;

            //ION - DESERIALIZE
            sw = Stopwatch.StartNew();
            Model ionDeserialized = ION.DeserializeObject<Model>(ion);
            sw.Stop();
            long ionDeserializeTime = sw.ElapsedTicks;
            string parity1 = ION.Parity(m);
            string parity2 = ION.Parity(ionDeserialized);

            if (parity1 != parity2)
            {
                throw new Exception(":-(");
            }

            //JSON - DESERIALIZE
            sw = Stopwatch.StartNew();
            Model jsonDeserialized = JsonConvert.DeserializeObject<Model>(jsonStr);
            sw.Stop();
            long jsonDeserializeTime = sw.ElapsedTicks;

            //SIZE
            Console.WriteLine("--SIZE--");
            Console.WriteLine("ION: " + ion.Length);
            Console.WriteLine("JSON: " + jsonStr.Length);
            long max = Math.Max(ion.Length, jsonStr.Length);
            long min = Math.Min(ion.Length, jsonStr.Length);
            float perc = ((float)max / (float)min);
            Console.WriteLine("DIFF: " + (ion.Length < jsonStr.Length ? "-" : "+") + (perc * 100f).ToString() + "% (" + perc.ToString() + "x)");

            //SPEED - SERIALIZE
            Console.WriteLine(Environment.NewLine + "--SERIALIZE SPEED--");
            Console.WriteLine("ION: " + ionSerializeTime);
            Console.WriteLine("JSON: " + jsonSerializeTime);
            max = Math.Max(ionSerializeTime, jsonSerializeTime);
            min = Math.Min(ionSerializeTime, jsonSerializeTime);
            perc = ((float)max / (float)min);
            Console.WriteLine("DIFF: " + (ionSerializeTime < jsonSerializeTime ? "-" : "+") + (perc * 100f).ToString() + "% (" + perc.ToString() + "x)");

            //SPEED - DESERIALIZE
            Console.WriteLine(Environment.NewLine + "--DESERIALIZE SPEED--");
            Console.WriteLine("ION: " + ionDeserializeTime);
            Console.WriteLine("JSON: " + jsonDeserializeTime);
            max = Math.Max(ionDeserializeTime, jsonDeserializeTime);
            min = Math.Min(ionDeserializeTime, jsonDeserializeTime);
            perc = ((float)max / (float)min);
            Console.WriteLine("DIFF: " + (ionDeserializeTime < jsonDeserializeTime ? "-" : "+") + (perc * 100f).ToString() + "% (" + perc.ToString() + "x)");

            Console.Read();
        }

        static void PopulateTestModel(ref Model m)
        {
            m.TestString = "123";
            m.TestInt = 123;
            m.TestFloat = 123f;
            m.TestDouble = 123.00;
            m.TestLong = 123;
            m.TestShort = 1;
            m.TestList = new List<int>();

            for (int i = 0; i < 99999; i++)
            {
                m.TestList.Add(12314);
            }

            m.TestArray = new float[] { 1.234f, 4.321f, 5.678f };
            m.TestSubClass = new SubClass();
            m.TestSubClass.SubclassTestString = "Test";
            m.TestDecimal = 1.00002m;
            m.TestByte = new byte[] { 128, 254 };

            m.TestSubClassArray = new SubClass[2];
            m.TestSubClassArray[0] = new SubClass();
            m.TestSubClassArray[0].SubclassTestString = "Test";
            m.TestSubClassArray[1] = null;

            m.TestEnum = TestEnum.Option2;
            m.TestBool = true;

            m.TestUShort = 256;
            m.TestUInt = 2147483648;
            m.TestULong = 9223372036854775808;
        }
    }
}
