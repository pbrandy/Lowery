using Lowery.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mappings
{
    internal static class TypeMapStore
    {
        internal static List<ITypeMapping> TypeMappings { get; set; } = SetDefaultTypeMappings();
        private static List<ITypeMapping> SetDefaultTypeMappings()
        {
            return new List<ITypeMapping>()
            {
                new TypeMapping<string?>(typeof(string), Convert.ToString),
                new TypeMapping<short>(typeof(short), Convert.ToInt16),
                new TypeMapping<int>(typeof(int), Convert.ToInt32),
                new TypeMapping<long>(typeof(long), Convert.ToInt64),
                new TypeMapping<float>(typeof(float), (obj) => (float)Convert.ToDecimal(obj)),
                new TypeMapping<double>(typeof(double), Convert.ToDouble),
                new TypeMapping<DateTime>(typeof(DateTime), Convert.ToDateTime),
                new TypeMapping<DateTime?>(typeof(DateTime?), (value) => { return (DateTime?)Convert.ToDateTime(value); }),
                new TypeMapping<Guid>(typeof(Guid), (obj) => Guid.Parse(Convert.ToString(obj) ?? ""))
            };
        }

        public static TypeMapping<T> GetMapping<T>()
		{
			var mapping = (TypeMapping<T>?)TypeMappings.FirstOrDefault(m => m.TargetType == typeof(T));
            if (mapping == null)
                throw new KeyNotFoundException($"Type mapping of type '{typeof(T).Name}' not found in registered type maps.");
            return mapping;
        }

        public static TypeMapping<T> GetMapping<T>(string name)
        {
            var mapping = (TypeMapping<T>?)TypeMappings.FirstOrDefault(m => m.Name == name);
            if (mapping == null)
                throw new KeyNotFoundException($"Type mapping with name '{name}' not found in registered type maps.");
            return mapping;
        }

        public static void AddMapping(ITypeMapping typeMapping)
        {
            TypeMappings.Add(typeMapping);
        }
    }
}
