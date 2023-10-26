using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	internal class LoweryFeatureDefinition
	{
		internal string Name { get; set; } = string.Empty;
		internal string DataSource { get; set; } = string.Empty;
		internal string Path { get; set; } = string.Empty;
		internal string? Parent { get; set; }
		internal FieldDefinition[]? MandatoryFields { get; set; }
		internal string? Style { get; set; }
		internal string? Registry { get; set; }
	}
}
