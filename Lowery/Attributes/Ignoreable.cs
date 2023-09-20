using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class Ignoreable : Attribute
    {
        public bool IgnoreGet { get; set; } = false;

        public Ignoreable()
        {
            
        }

        public Ignoreable(bool ignoreGet)
        {
            IgnoreGet = ignoreGet;
        }
    }
}
