using System;

namespace MainWindow
{
    public static class Core
    {
        internal static long LongRandom(long min, long max)
        {
            Random rand = new Random();
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
        internal static int IntRandom(int min, int max)
        {
            Random rand = new Random();
            int result = rand.Next(min, max);
            return result;
        }
        internal static bool ResultRandom(int percentage)
        {
            long result = IntRandom(0, 100);
            return result < percentage;
        }

    }
}
