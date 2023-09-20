using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Internal
{
    internal static class Common
    {
        internal static Dictionary<string, PropertyInfo[]> SortPropertyInfo(Type t)
        {
            Dictionary<string, PropertyInfo[]> results = new Dictionary<string, PropertyInfo[]>();
            var properties = t.GetProperties();

            var primaries = properties.Where(p => p.GetCustomAttribute<PrimaryKey>() != null).ToArray();
            results.Add("PrimaryKey", primaries);

            var related = properties.Where(p => p.GetCustomAttribute<Related>() != null).ToArray();
            results.Add("Related", related);

            var test = properties[3].GetCustomAttribute<PrimaryKey>();
            var standard = properties.Where(p => p.CustomAttributes.Count() == 0).ToArray();

            results.Add("StandardProps", standard);
            return results;
        }

        internal static void Dispose(IEnumerable<Row> rows)
        {
            foreach (Row row in rows)
                row.Dispose();
        }
    }
}
