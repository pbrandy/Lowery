using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public class LoweryFieldDefinition
	{
		internal string Field { get; set; } = string.Empty;
		internal string? Alias { get; set; }
		internal FieldType Type { get; set; }
	}
}
