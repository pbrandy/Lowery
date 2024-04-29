using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Mapping;
using Lowery.Internal;
using Lowery.Mappings;
using System.Reflection;

namespace Lowery
{
	public partial class LoweryBase
	{
		public MapMember? MapMember { get; set; }
		public IDisplayTable? DisplayTable { get; set; }

		public async Task<IEnumerable<T>> Get<T>(string whereClause = "") where T : class, new()
		{
			QueryFilter filter = new QueryFilter
			{
				WhereClause = whereClause
			};
			if (DisplayTable == null)
				return new List<T>();
            using Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
			return await table.Get<T>(filter);
        }

		public async Task<IEnumerable<T>> Get<T>(QueryFilter? filter) where T : class, new()
		{
			if (DisplayTable == null)
				return new List<T>();
			using Table table = await QueuedTask.Run(() => { return DisplayTable.GetTable(); });
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
            RowToken token = editOperation.Create(MapMember, attributes);
            await editOperation.ExecuteAsync();

			foreach (var prop in propInfo["GlobalIdentifier"])
			{
				prop.PropertyInfo.SetValue(value, token.GlobalID);
			}
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
            Dictionary<string, object> attributes = new();
            foreach (var prop in propInfo["StandardProps"])
            {
				var customMapping = propInfo["CustomMappings"].FirstOrDefault(p => p.FieldName == prop.FieldName);
				if (customMapping != null)
				{
					var mapping = TypeMapStore.GetMapping(customMapping.Mapping);
					var convertedValue = mapping.ConvertToDatabase.Invoke(customMapping.PropertyInfo.GetValue(value));
					attributes.Add(prop.FieldName, convertedValue);
				}
				else
				{
					attributes.Add(prop.FieldName, prop.PropertyInfo.GetValue(value));
				}
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

		public async Task DeleteMany(QueryFilter filter)
		{
			if (MapMember == null)
				return;

			await QueuedTask.Run(async () =>
			{
				var rowsToDelete = ((IDisplayTable)MapMember).Select(filter).GetObjectIDs();
				EditOperation editOperation = new();
				editOperation.Name = $"Delete record from {MapMember.Name}";
				editOperation.Delete(MapMember, rowsToDelete);
				await editOperation.ExecuteAsync();
			});
		}
    }
}
