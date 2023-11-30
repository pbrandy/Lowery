using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Mapping;
using Lowery.Internal;
using Lowery.Mappings;

namespace Lowery
{
	public partial class LoweryBase
	{
		public MapMember? MapMember { get; set; }
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

        public async Task<long?> Insert<T>(T value) where T : class, new()
		{
            if (MapMember == null)
                return null;
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));

            // Attributes
            Dictionary<string, object?> attributes = new();
            foreach (var prop in propInfo["StandardProps"])
            {
                attributes.Add(prop.FieldName, prop.PropertyInfo.GetValue(value));
            }

            //Insert
            EditOperation editOperation = new();
            editOperation.Name = $"Insert record into {MapMember.Name}";
            RowToken token = editOperation.Create(MapMember);
            await editOperation.ExecuteAsync();
            return token.ObjectID;
        }

		public async Task Update<T>(T value) where T : class, new()
		{
            if (MapMember == null)
                return;
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));
            long? oid = (long?)(propInfo["PrimaryKey"][0]?.PropertyInfo.GetValue(value));
			if (oid == null)
				return;

            EditOperation editOperation = new EditOperation();
            editOperation.Name = $"Update record in {MapMember.Name}";

            // Attributes
            Dictionary<string, object?> attributes = new();
            foreach (var prop in propInfo["StandardProps"])
            {
                attributes.Add(prop.FieldName, prop.PropertyInfo.GetValue(value));
            }

            editOperation.Modify(MapMember, (long)oid, attributes);
            await editOperation.ExecuteAsync();
        }
		public async Task Update(QueryFilter filter)
		{
			if (MapMember == null)
				return;

			EditOperation editOperation = new EditOperation();
			editOperation.Name = $"Update record in {MapMember.Name}";
			await editOperation.ExecuteAsync();
		}

		public async Task Delete<T>(T value) where T : class, new()
		{
			if (MapMember == null)
				return;
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));
            long? oid = (long?)(propInfo["PrimaryKey"][0]?.PropertyInfo.GetValue(value));
            if (oid == null)
                return;

            EditOperation editOperation = new();
            editOperation.Name = $"Delete record from {MapMember.Name}";
            editOperation.Delete(MapMember, (long)oid);
            await editOperation.ExecuteAsync();
        }

		public async Task Delete(QueryFilter filter)
		{
			if (MapMember == null)
				return;
			Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));

            var rowsToDelete = ((IDisplayTable)MapMember).Select(filter).GetObjectIDs();
			EditOperation editOperation = new();
			editOperation.Name = $"Delete record from {MapMember.Name}";
			editOperation.Delete(MapMember, rowsToDelete);
			await editOperation.ExecuteAsync();
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
