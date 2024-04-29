using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mappings
{
	public class TypeMappingBase : ITypeMapping
	{
		public string Name { get; set; }
		public Type TargetType { get; set; }
		public Func<object, object> ConvertToDatabase { get; set; }
	}
}
