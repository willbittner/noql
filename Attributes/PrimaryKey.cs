using System;
using System.Windows.Forms.VisualStyles;

namespace Dashx.CEP.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class PrimaryKeyAttribute : System.Attribute
    {
        public PrimaryKeyAttribute()
            : base()
        {
            
        }
    }
}
