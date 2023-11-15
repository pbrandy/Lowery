using ArcGIS.Core.Data;

namespace Lowery.Definitions
{
    public class LoweryFieldDefinition
    {
        public string Field { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public FieldType Type { get; set; }
    }
}
