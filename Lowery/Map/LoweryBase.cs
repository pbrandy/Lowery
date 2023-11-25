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

		public async Task<long> Insert<T>(T value) where T : class, new()
		{
            if (DisplayTable == null)
                return -1;
            Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
			return await table.Insert<T>(value);
        }

		public async Task Update<T>(T value) where T : class, new()
		{
            if (DisplayTable == null)
                return;
            Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
            await table.Update<T>(value);
        }

		public async Task Delete<T>(T value) where T : class, new()
		{
			if (DisplayTable == null)
				return;
			Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
			await table.Delete<T>(value);
		}

		public async Task DeleteMany<T>(IEnumerable<T> values) where T : class, new()
		{
            if (DisplayTable == null)
                return;
            Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
			//await table.DeleteMany<T>(values);
        }
    }
}
