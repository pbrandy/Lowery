using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public partial class LoweryBase
	{
		public IDisplayTable? DisplayTable { get; set; }

		public async Task<IEnumerable<T>> Get<T>(string whereClause = "") where T : class, new()
		{
			if (DisplayTable == null)
				return new List<T>();
			return await DisplayTable.GetTable().Get<T>(whereClause);
		}

		public async Task<IEnumerable<T>> Get<T>(QueryFilter? filter) where T : class, new()
		{
			if (DisplayTable == null)
				return new List<T>();
			return await DisplayTable.GetTable().Get<T>(filter);
		}
	}
}
