using System;

namespace OpenRCT2.API.Diagnostics
{
    public static class Guard
    {
        public static void ArgumentNotNull<T>(T arg) where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
