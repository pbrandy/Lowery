using Lowery.Mapping;
using Lowery.Mappings;

namespace Lowery
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CustomMapping : Attribute
	{
        public string Mapping { get; set; }

        public CustomMapping(string mapName)
        {
            Mapping = mapName;
        }
    }
}
