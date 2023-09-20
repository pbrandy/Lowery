using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FieldName : Attribute
    {
        public string Name { get; set; }

        public FieldName(string name)
        {
            Name = name;
        }
    }
}
