using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Lowery
{
	public partial class LoweryBase
	{
		public IDisplayTable? DisplayTable { get; set; }

		public async Task<IEnumerable<T>> Get<T>(string whereClause = "") where T : class, new()
		{
			if (DisplayTable == null)
				return new List<T>();
			Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
			return await table.Get<T>(whereClause);
        }

		public async Task<IEnumerable<T>> Get<T>(QueryFilter? filter) where T : class, new()
		{
			if (DisplayTable == null)
				return new List<T>();
            Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
            return await table.Get<T>(filter);
		}
	}
}
