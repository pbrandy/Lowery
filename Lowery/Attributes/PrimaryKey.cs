using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKey : Attribute
    {
        public string? FieldName { get; set; }
        public PrimaryKey()
        {

        }

        public PrimaryKey(string name)
        {
            FieldName = name;
        }
    }
}
