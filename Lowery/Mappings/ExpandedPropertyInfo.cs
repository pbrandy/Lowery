using System.Reflection;

namespace Lowery.Mappings
{
	internal class ExpandedPropertyInfo
	{
		public bool IsPrimaryKey { get; set; } = false;
		public bool IsIgnorable { get; set; } = false;
		public bool IsRelational { get; set; } = false;
		public string FieldName { get; set; }
		public string RelationName { get; set; }
		public string Mapping { get; set; }
		public PropertyInfo PropertyInfo { get; set; }
        public ExpandedPropertyInfo(string name, PropertyInfo propertyInfo)
        {
			FieldName = name;
			PropertyInfo = propertyInfo;
        }
    }
}
