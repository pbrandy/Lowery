using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Lowery.Internal;
using Lowery.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Extensions
{
	public static class FeatureLayerExtensions
	{
		public static async Task<IEnumerable<T>> Get<T>(this FeatureLayer layer, string whereClause = "") where T : class, new()
		{
			QueryFilter filter = new QueryFilter() { WhereClause = whereClause };
			Table table = await QueuedTask.Run(() => { return layer.GetTable(); });
			return await TableExtensions.Get<T>(table, filter);
		}

		public static async Task<IEnumerable<T>> Get<T>(this FeatureLayer layer, QueryFilter? queryFilter) where T : class, new()
		{
			Table table = await QueuedTask.Run(() => { return layer.GetTable(); });
			return await TableExtensions.Get<T>(table, queryFilter);
		}


	}
}
