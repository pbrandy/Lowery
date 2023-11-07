using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public class LoweryFeatureDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string DataSource { get; set; } = string.Empty;
		public string Path { get; set; } = string.Empty;
		public string? Parent { get; set; }
		public LoweryFieldDefinition[]? MandatoryFields { get; set; }
		public string? Style { get; set; }
		public string? Registry { get; set; }
	}
}
