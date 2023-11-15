using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lowery.Definitions;

namespace Lowery
{
    public enum DataSourceType { FGDB, EGDB, SHP }

	public class DataSource
	{
        public string Name { get; set; } = string.Empty;
		public DataSourceType DataSourceType { get; set; }
		public string Path { get; set; } = string.Empty;

		public DataSource(LoweryDataSourceDefinition definition)
		{
			Name = definition.Name;
			DataSourceType = Enum.Parse<DataSourceType>(definition.Type);
			Path = definition.Path;
		}
    }
}
