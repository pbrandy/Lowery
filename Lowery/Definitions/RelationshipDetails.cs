using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Lowery.Mappings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Definitions
{
    internal class RelationshipDetails
    {
        public RelationshipClass RelationshipClass { get; set; }
        public Type RelatedType { get; set; }
        public bool IsEnumerable { get; set; }

        private MethodInfo? methodInfo;
        public MethodInfo? GetMethod => methodInfo ??= GenerateGet();

        public RelationshipDetails(RelationshipClass relationshipClass, Type relatedType, bool isEnumerable)
        {
            RelationshipClass = relationshipClass;
            RelatedType = relatedType;
            IsEnumerable = isEnumerable;
        }

        private MethodInfo? GenerateGet()
        {
            return typeof(RelationshipExtentions)
                .GetMethod("GetRelated")?
                .MakeGenericMethod(new Type[] { RelatedType });
        }

        public async Task<IEnumerable<object>?> Get(long oid, string tableName)
        {
            if (GetMethod?.Invoke(this, new object[] { RelationshipClass, oid, tableName }) is Task task)
            {
                await task.ConfigureAwait(false);
                return (IEnumerable<object>?)task.GetType().GetProperty("Result")?.GetValue(task);
            }
            return null;
        }

        public static async Task<RelationshipDetails> Create(Table table, ExpandedPropertyInfo prop)
        {
            return await QueuedTask.Run(() => {
                using Geodatabase gdb = (Geodatabase)table.GetDatastore();

                var relationship = gdb.OpenDataset<RelationshipClass>(prop.RelationName);
                bool relatedIsEnumerable = false;
                Type relatedType = prop.PropertyInfo.PropertyType;
                if (typeof(IEnumerable).IsAssignableFrom(relatedType))
                {
                    relatedType = relatedType.GetGenericArguments()[0];
                    relatedIsEnumerable = true;
                }

                return new RelationshipDetails(relationship, relatedType, relatedIsEnumerable);
            });
        }


    }
}
