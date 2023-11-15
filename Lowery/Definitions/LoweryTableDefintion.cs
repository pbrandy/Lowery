using ArcGIS.Desktop.Mapping;
using Lowery.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lowery.Definitions
{
    public class LoweryTableDefintion : ILoweryDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
        public string? Parent { get; set; }
        public string Path { get; set; } = string.Empty;
        public LoweryFieldDefinition[]? MandatoryFields { get; set; }
        public string? Registry { get; set; }
        internal LoweryStandaloneTable? FindValid(IEnumerable<StandaloneTable> tables)
        {
            List<StandaloneTable> matches = tables.Where(l => l.Name == Name && l.GetParentName() == Parent).ToList();
            if (matches.Count() == 0)
                return null;

            if (MandatoryFields == null || MandatoryFields.Length == 0)
                return new LoweryStandaloneTable(this, matches.First());

            for (int i = 0; i < MandatoryFields?.Length; i++)
            {
                LoweryFieldDefinition field = MandatoryFields[i];

                foreach (var match in matches)
                {
                    var descriptions = match.GetFieldDescriptions();
                    var validField = descriptions.FirstOrDefault(d => d.Name == field.Field && d.Type == field.Type && d.Alias == field.Alias);
                    if (validField != null)
                        return new LoweryStandaloneTable(this, match);
                }
            }
            return null;
        }
    }
}
