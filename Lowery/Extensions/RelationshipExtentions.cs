using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Lowery.Internal;
using Lowery.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public static class RelationshipExtentions
	{

		public static async Task<IEnumerable<T>> GetRelated<T>(this RelationshipClass relationship, long oid, string source) where T : class, new()
		{
			return await QueuedTask.Run(() =>
			{
				using Geodatabase gdb = (Geodatabase)relationship.GetDatastore();
				RelationshipClassDefinition def = relationship.GetDefinition();
				Table targetTable;
				if (def.GetOriginClass() == source)
					targetTable = gdb.OpenDataset<Table>(def.GetDestinationClass());
				else
					targetTable = gdb.OpenDataset<Table>(def.GetOriginClass());

				return targetTable.Get<T>($"{def.GetOriginForeignKeyField()} = {oid}", new string[] { def.GetName() });
			});
		}

		public static async Task UpdatedRelated<SourceT, RelatedT>(this RelationshipClass relationship, RelatedT related)
			where RelatedT : class, new()
		{
			Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(SourceT));

			await QueuedTask.Run(() =>
			{
				var definition = relationship.GetDefinition();
			});
		}
	}
}
