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

        public static IEnumerable<T> GetRelated<T>(this RelationshipClass relationship, long oid) where T : class, new()
        {
            Dictionary<string, List<ExpandedPropertyInfo>> propInfo = Common.SortPropertyInfo(typeof(T));

            return QueuedTask.Run(() => {
                Geodatabase gdb = (Geodatabase)relationship.GetDatastore();
                IReadOnlyList<Row> relatedRows = relationship.GetRowsRelatedToOriginRows(new long[] { oid });
                Table relatedTable = gdb.OpenDataset<Table>(relationship.GetDefinition().GetDestinationClass());
                RelationshipClassDefinition def = relationship.GetDefinition();

                return relatedTable.Get<T>($"{def.GetOriginForeignKeyField()} = {oid}");
            }).Result;
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
