using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	internal class LoweryTableDefintion : ILoweryDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string DataSource { get; set; } = string.Empty;
		public string? Parent { get; set; }
		public string Path { get; set; } = string.Empty;
		public LoweryFieldDefinition[]? MandatoryFields { get; set; }
		public string? Registry { get; set; }
	}
}
