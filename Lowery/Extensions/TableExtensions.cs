using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Lowery.Definitions;
using Lowery.Internal;
using Lowery.Mappings;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lowery
{
	public static class TableExtensions
	{
		public static async Task<IEnumerable<T>> Get<T>(this Table table, string whereClause = "", IEnumerable<string>? relationshipToIgnore = null) where T : class, new()
		{
			QueryFilter filter = new QueryFilter
			{
				WhereClause = whereClause
			};
			return await table.Get<T>(filter, relationshipToIgnore);
		}

		public static async Task<IEnumerable<T>> Get<T>(this Table table, QueryFilter? queryFilter, IEnumerable<string>? relationshipToIgnore = null) where T : class, new()
		{
			List<T> results = new List<T>();
			Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));
			long? oid = null;
			return await QueuedTask.Run(async delegate
			{
				using RowCursor cursor = table.Search(queryFilter);
				while (cursor.MoveNext())
				{
					T newEntity = new T();
					using Row row = cursor.Current;
					foreach (ExpandedPropertyInfo prop in propInfo["StandardProps"].Concat(propInfo["PrimaryKey"]))
					{
						Map(newEntity, row, prop);
						if (prop.IsPrimaryKey)
						{
							oid = (long?)prop.PropertyInfo.GetValue(newEntity);
						}
					}
					foreach (ExpandedPropertyInfo prop2 in propInfo["Related"])
					{
						if (oid.HasValue && (relationshipToIgnore == null || !relationshipToIgnore.Contains<string>(prop2.RelationName)))
						{
							RelationshipDetails details = await RelationshipDetails.Create(table, prop2);
							IEnumerable<object> result = await details.Get(oid.Value, table.GetName());
							if (details.IsEnumerable)
							{
								prop2.PropertyInfo.SetValue(newEntity, result);
							}
							else
							{
								prop2.PropertyInfo.SetValue(newEntity, result?.FirstOrDefault());
							}
						}
					}
					results.Add(newEntity);
				}
				return results;
			});
		}

		internal static void Map<T>(T newEntity, Row row, ExpandedPropertyInfo prop) where T : class
		{
			MethodInfo methodInfo = typeof(TypeMapStore).GetMethod("GetMapping", 1, Array.Empty<Type>())?.MakeGenericMethod(prop.PropertyInfo.PropertyType);
			if (!(methodInfo == null))
			{
				object mapping = methodInfo.Invoke(null, null);
				if (mapping != null)
				{
					MethodInfo gentype = mapping.GetType().GetMethod("Execute");
					prop.PropertyInfo.SetValue(newEntity, gentype?.Invoke(mapping, new object[1] { row[prop.FieldName] }));
				}
			}
		}

		public static async Task<long> Insert<T>(this Table table, T value) where T : class, new()
		{
			Table table2 = table;
			T value2 = value;
			List<PropertyInfo> propInfo = typeof(T).GetProperties().ToList();
			propInfo.RemoveAll((PropertyInfo p) => p.GetCustomAttribute<PrimaryKey>() != null || p.GetCustomAttribute<Ignoreable>() != null);
			return await QueuedTask.Run(delegate
			{
				using InsertCursor insertCursor = table2.CreateInsertCursor();
				using RowBuffer rowBuffer = table2.CreateRowBuffer();
				foreach (PropertyInfo current in propInfo)
				{
					rowBuffer[current.Name] = current.GetValue(value2);
				}
				long result = insertCursor.Insert(rowBuffer);
				insertCursor.Flush();
				return result;
			});
		}

		public static async Task Update<T>(this Table table, T value) where T : class, new()
		{
			Table table2 = table;
			T value2 = value;
			PropertyInfo[] propertyInfos = typeof(T).GetProperties();
			PropertyInfo primaryKeyInfo = propertyInfos.Where((PropertyInfo p) => Attribute.IsDefined(p, typeof(PrimaryKey))).FirstOrDefault();
			if (primaryKeyInfo == null)
			{
				throw new ArgumentNullException("Primary Key Missing", "Could not locate primary key for object of type " + typeof(T).Name + ". Specify a primary key with a primary key attribute.");
			}
			long? key = primaryKeyInfo.GetValue(value2) as long?;
			await QueuedTask.Run(delegate
			{
				EditOperation editOperation = new EditOperation();
				editOperation.Callback(delegate (EditOperation.IEditContext context)
				{
					QueryFilter queryFilter = new QueryFilter();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler.AppendLiteral("OBJECTID = ");
					defaultInterpolatedStringHandler.AppendFormatted(key);
					queryFilter.WhereClause = defaultInterpolatedStringHandler.ToStringAndClear();
					QueryFilter queryFilter2 = queryFilter;
					using RowCursor rowCursor = table2.Search(queryFilter2);
					while (rowCursor.MoveNext())
					{
						using Row row = rowCursor.Current;
						context.Invalidate(row);
						PropertyInfo[] array = propertyInfos;
						foreach (PropertyInfo propertyInfo in array)
						{
							row[propertyInfo.Name] = propertyInfo.GetValue(value2) as T;
						}
						row.Store();
						context.Invalidate(row);
					}
				}, table2);
			});
		}

		public static async Task Delete<T>(this Table table, T value) where T : class, new()
		{
			Table table2 = table;
			PropertyInfo[] propertyInfos = typeof(T).GetProperties();
			PropertyInfo primaryKeyInfo = propertyInfos.Where((PropertyInfo p) => Attribute.IsDefined(p, typeof(PrimaryKey))).FirstOrDefault();
			if (primaryKeyInfo == null)
			{
				throw new ArgumentNullException("Primary Key Missing", "Could not locate primary key for object of type " + typeof(T).Name + ". Specify a primary key with a primary key attribute.");
			}
			long? key = primaryKeyInfo.GetValue(value) as long?;
			await QueuedTask.Run(delegate
			{
				EditOperation editOperation = new EditOperation();
				editOperation.Callback(delegate (EditOperation.IEditContext context)
				{
					QueryFilter queryFilter = new QueryFilter();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
					defaultInterpolatedStringHandler.AppendLiteral("objectid = '");
					defaultInterpolatedStringHandler.AppendFormatted(key);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					queryFilter.WhereClause = defaultInterpolatedStringHandler.ToStringAndClear();
					QueryFilter queryFilter2 = queryFilter;
					using RowCursor rowCursor = table2.Search(queryFilter2, useRecyclingCursor: false);
					while (rowCursor.MoveNext())
					{
						using Row row = rowCursor.Current;
						context.Invalidate(row);
						row.Delete();
					}
				}, table2);
			});
		}

		public static async Task DeleteMany(this Table table, QueryFilter queryFilter)
		{
			Table table2 = table;
			QueryFilter queryFilter2 = queryFilter;
			await QueuedTask.Run(delegate
			{
				EditOperation editOperation = new EditOperation();
				editOperation.Callback(delegate (EditOperation.IEditContext context)
				{
					using RowCursor rowCursor = table2.Search(queryFilter2, useRecyclingCursor: false);
					while (rowCursor.MoveNext())
					{
						using Row row = rowCursor.Current;
						context.Invalidate(row);
						row.Delete();
					}
				}, table2);
			});
		}
	}
}
