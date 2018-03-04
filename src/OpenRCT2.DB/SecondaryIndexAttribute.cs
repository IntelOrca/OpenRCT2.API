using System;

namespace OpenRCT2.DB
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SecondaryIndexAttribute : Attribute
    {
    }
}
