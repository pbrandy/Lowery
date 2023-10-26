using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Lowery.Internal;
using Lowery.Mappings;
using System.Reflection;

namespace Lowery
{
    public static class TableExtensions
    {
        public static async Task<IEnumerable<T>> Get<T>(this Table table, string whereClause = "") where T : class, new()
        {
            QueryFilter filter = new QueryFilter() { WhereClause = whereClause };
            return await Get<T>(table, filter);
        }

        private static async Task<IEnumerable<T>> Get<T>(this Table table, QueryFilter? queryFilter) where T : class, new()
        {
            List<T> results = new();
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));
            
            await QueuedTask.Run(() =>
            {
                using RowCursor cursor = table.Search(queryFilter);
                while (cursor.MoveNext())
                {
                    T newEntity = new();
                    using Row row = cursor.Current;

                    foreach (var prop in propInfo["StandardProps"].Concat(propInfo["PrimaryKey"]))
                    {
                        var method = typeof(TypeMapStore).GetMethod("GetMapping", 1, Array.Empty<Type>());
                        var methodInfo = method?.MakeGenericMethod(prop.PropertyInfo.PropertyType);
                        if (methodInfo == null)
                            continue;

                        var mapping = methodInfo.Invoke(null, null);
                        if (mapping == null)
                            continue;
                        MethodInfo? gentype = mapping.GetType().GetMethod("Execute");

                        prop.PropertyInfo.SetValue(
                            newEntity,
                            gentype?.Invoke(mapping, new object[] { row[prop.FieldName] })
                            );
                    }
                    results.Add(newEntity);
                }
            });
            return results;
        }

        public static async Task<long> Insert<T>(this Table table, T value) where T : class, new()
        {
            List<PropertyInfo> propInfo = typeof(T).GetProperties()
                .ToList();

            propInfo.RemoveAll(p => p.GetCustomAttribute<PrimaryKey>() is not null || p.GetCustomAttribute<Ignoreable>() is not null);

            return await QueuedTask.Run(() =>
            {
                using var insertCursor = table.CreateInsertCursor();
                using var rowbuffer = table.CreateRowBuffer();
                foreach (var prop in propInfo)
                {
                    rowbuffer[prop.Name] = prop.GetValue(value);
                }
                long objectID = insertCursor.Insert(rowbuffer);
                insertCursor.Flush();
                return objectID;
            });
        }

        public static async Task Update<T>(this Table table, T value) where T : class, new()
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            PropertyInfo? primaryKeyInfo = propertyInfos.Where(p => Attribute.IsDefined(p, typeof(PrimaryKey))).FirstOrDefault();
            if (primaryKeyInfo == null)
                throw new ArgumentNullException("Primary Key Missing", $"Could not locate primary key for object of type {typeof(T).Name}. Specify a primary key with a primary key attribute.");
            long? key = primaryKeyInfo.GetValue(value) as long?;

            await QueuedTask.Run(() =>
            {
                EditOperation editOperation = new EditOperation();
                editOperation.Callback(context =>
                {
                    QueryFilter filter = new QueryFilter()
                    {
                        WhereClause = $"OBJECTID = {key}"
                    };
                    using RowCursor rowCursor = table.Search(filter);
                    while (rowCursor.MoveNext())
                    {
                        using var row = rowCursor.Current;
                        context.Invalidate(row);

                        foreach (var prop in propertyInfos)
                        {
                            row[prop.Name] = prop.GetValue(value) as T;
                        }

                        row.Store();
                        context.Invalidate(row);
                    }
                }, table);
            });
        }

        public static async Task Delete<T>(this Table table, T value) where T : class, new()
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            PropertyInfo? primaryKeyInfo = propertyInfos.Where(p => Attribute.IsDefined(p, typeof(PrimaryKey))).FirstOrDefault();
            if (primaryKeyInfo == null)
                throw new ArgumentNullException("Primary Key Missing", 
                    $"Could not locate primary key for object of type {typeof(T).Name}. Specify a primary key with a primary key attribute.");
            long? key = primaryKeyInfo.GetValue(value) as long?;

            await QueuedTask.Run(() =>
            {

                EditOperation editOperation = new EditOperation();
                editOperation.Callback(context =>
                {
                    QueryFilter filter = new() { WhereClause = $"objectid = '{key}'" };

                    using RowCursor rowCursor = table.Search(filter, false);
                    while (rowCursor.MoveNext())
                    {
                        using Row row = rowCursor.Current;
                        context.Invalidate(row);
                        row.Delete();
                    }
                }, table);
            });
        }

        public static async Task DeleteMany(this Table table, QueryFilter queryFilter)
        {
            await QueuedTask.Run(() => {

                EditOperation editOperation = new EditOperation();
                editOperation.Callback(context =>
                {
                    using RowCursor rowCursor = table.Search(queryFilter, false);
                    while (rowCursor.MoveNext())
                    {
                        using Row row = rowCursor.Current;
                        context.Invalidate(row);
                        row.Delete();
                    }
                }, table);
            });
        }
    }
}
