using System;
using System.Text;

namespace OpenRCT2.Content.Extensions
{
    public static class StringExtensions
    {
        public static string ToSpaceyString(this Enum e)
        {
            var text = e.ToString();
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (char.IsUpper(c))
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(' ');
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
