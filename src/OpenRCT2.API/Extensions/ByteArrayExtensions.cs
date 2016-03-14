using System;

namespace OpenRCT2.API.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            char[] szToken = new char[bytes.Length * 2];
            int szTokenIndex = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                szToken[szTokenIndex++] = ToHex(b >> 4);
                szToken[szTokenIndex++] = ToHex(b & 0x0F);
            }
            return new String(szToken);
        }

        private static char ToHex(int x)
        {
            if (x >= 0)
            {
                if (x <= 9) return (char)('0' + x);
                if (x <= 15) return (char)('a' + x - 10);
            }
            throw new ArgumentOutOfRangeException(nameof(x));
        }
    }
}
