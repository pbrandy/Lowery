using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	internal class LoweryFieldDefinition
	{
		internal string Field { get; set; } = string.Empty;
		internal string? Alias { get; set; }
		internal string Type { get; set; } = string.Empty;
	}
}
