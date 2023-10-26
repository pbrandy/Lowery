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
        public static async Task GetRelated<SourceT, RelatedT>(this RelationshipClass relationship, SourceT origin)
            where RelatedT : class, new()
        {
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(SourceT));

            if (propInfo["Related"].Count == 0)
                return;

            await QueuedTask.Run(() => {
                long? primaryKey = propInfo["PrimaryKey"][0].PropertyInfo.GetValue(origin) as long?;
                if (primaryKey == null)
                    throw new ArgumentNullException(nameof(primaryKey), $"Could not locate primary key for object of type {typeof(SourceT).Name}. " +
                        $"Please specify a primary key property of the type long using the attribute.");
                var relatedRows = relationship.GetRowsRelatedToOriginRows(new long[] {(long)primaryKey});

                var relatedProp = propInfo["Related"].FirstOrDefault(p => p.PropertyInfo.GetCustomAttribute<Related>()?.RelationshipClass == relationship.GetName());
                if (relatedProp == null)
                    return;

                Dictionary<string, List<ExpandedPropertyInfo>> relatedInfo = Common.SortPropertyInfo(typeof(RelatedT));
                if (relatedProp.PropertyInfo.PropertyType == typeof(List<RelatedT>))
                {
                    List<RelatedT> relatedData = new List<RelatedT>();
                    foreach (var relatedRow in relatedRows)
                    {
                        var newEntity = new RelatedT();

                        foreach (var prop in relatedInfo["StandardProps"].Concat(propInfo["PrimaryKey"]))
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
                                gentype?.Invoke(mapping, new object[] { relatedRow[prop.FieldName] })
                                );
                        }
                        relatedData.Add(newEntity);
                    }
                    relatedProp.PropertyInfo.SetValue(origin, relatedData);
                }
                Common.Dispose(relatedRows);
            });
        }

        public static async Task UpdatedRelated<SourceT, RelatedT>(this RelationshipClass relationship, RelatedT related)
            where RelatedT : class, new()
        {
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(SourceT));

            await QueuedTask.Run(() => {
                var definition = relationship.GetDefinition();
            });
        }
    }
}
