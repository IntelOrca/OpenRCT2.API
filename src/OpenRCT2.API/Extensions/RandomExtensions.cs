using System;

namespace OpenRCT2.API.Extensions
{
    public static class RandomExtensions
    {
        public static byte[] NextBytes(this Random random, int count)
        {
            byte[] buffer = new byte[count];
            random.NextBytes(buffer);
            return buffer;
        }
    }
}
