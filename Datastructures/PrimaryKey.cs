using System;

namespace NoQL.CEP.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute()
            : base()
        {
        }
    }
}