using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	internal class LoweryFeatureDefinition : ILoweryDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string DataSource { get; set; } = string.Empty;
		public string Path { get; set; } = string.Empty;
		public string? Parent { get; set; }
		public FieldDefinition[]? MandatoryFields { get; set; }
		public string? Style { get; set; }
		public string? Registry { get; set; }
	}
}
