using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mappings
{
	internal class ExpandedPropertyInfo
	{
		public bool IsPrimaryKey { get; set; } = false;
		public bool IsIgnorable { get; set; } = false;
		public bool IsRelational { get; set; } = false;
		public string FieldName { get; set; }
		public PropertyInfo PropertyInfo { get; set; }
        public ExpandedPropertyInfo(string name, PropertyInfo propertyInfo)
        {
			FieldName = name;
			PropertyInfo = propertyInfo;
        }
    }
}
