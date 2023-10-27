using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	internal enum DataSourceType { FGDB, EGDB, SHP }
	internal class DataSource
	{
        public string Name { get; set; } = string.Empty;
		public DataSourceType DataSourceType { get; set; }
		public string Path { get; set; } = string.Empty;

		public DataSource()
		{

		}

		public DataSource(LoweryDataSourceDefinition definition)
		{
			Name = definition.Name;
			DataSourceType = Enum.Parse<DataSourceType>(definition.Type);
			Path = definition.Path;
		}
    }
}
