using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public interface ILoweryItem
	{
		string Name { get; }
		public string Uri { get; set; }
		public DataSource DataSource { get; set; }
		public string? Parent { get; set; }
        public ItemRegistry? Registry { get; set; }
		public IDisplayTable? DisplayTable { get; set; }
		public IEnumerable<LoweryFieldDefinition>? MandatoryFields { get; set; }
		
	}
}
