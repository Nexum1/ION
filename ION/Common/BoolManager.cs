using System;
using System.Collections.Generic;

namespace IONConvert
{
    static class BoolManager
    {
        static List<BoolWatcher> Bools;
        static Queue<bool> DeserializedBools;

        public static void Reset()
        {
            Bools = new List<BoolWatcher>();
            DeserializedBools = new Queue<bool>();
        }

        public static bool NextBool(byte b, ref int index)
        {
            if (DeserializedBools.Count == 0)
            {
                bool[] bools = new bool[8];

                for (int i = 0; i < 8; i++)
                    bools[i] = (b & (1 << i)) == 0 ? false : true;

                Array.Reverse(bools);

                for (int i = 0; i < 8; i++)
                {
                    DeserializedBools.Enqueue(bools[i]);
                }
                index += 1;
            }

            return DeserializedBools.Dequeue();
        }

        public static BoolEditResult AddBool(bool b, ref int index)
        {
            BoolWatcher freeWatcher = null;

            foreach (BoolWatcher watcher in Bools)
            {
                if (watcher.SpaceLeft)
                {
                    freeWatcher = watcher;
                    break;
                }
            }

            if (freeWatcher == null)
            {
                freeWatcher = new BoolWatcher() { Position = index };
                Bools.Add(freeWatcher);
                index += 1;
            }

            freeWatcher.AddBool(b);

            return new BoolEditResult { Postion = freeWatcher.Position, Bool = freeWatcher.Byte };
        }

        public static void UpdateBools(ref List<byte> ion)
        {
            foreach (BoolWatcher watcher in Bools)
            {
                ion.Insert(watcher.Position, watcher.Byte);
            }
        }

        class BoolWatcher
        {
            public int Position = 0;
            bool[] Bools = new bool[8];
            int BoolsInserted = 0;

            public bool SpaceLeft
            {
                get
                {
                    return BoolsInserted < 8;
                }
            }

            public void AddBool(bool b)
            {
                Bools[BoolsInserted] = b;
                BoolsInserted++;
            }

            public byte Byte
            {
                get
                {
                    byte result = 0;
                    int index = 8 - Bools.Length;

                    foreach (bool b in Bools)
                    {
                        if (b)
                            result |= (byte)(1 << (7 - index));

                        index++;
                    }

                    return result;
                }
            }
        }
    }

    public struct BoolEditResult
    {
        public int Postion;
        public byte Bool;
    }
}