using Lowery.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mappings
{
    public static class TypeMapStore
    {
        public static List<ITypeMapping> TypeMappings { get; set; } = SetDefaultTypeMappings();

        private static List<ITypeMapping> SetDefaultTypeMappings()
        {
            return new List<ITypeMapping>()
            {
                new TypeMapping<string?>(typeof(string), Convert.ToString),
                new TypeMapping<short>(typeof(short), Convert.ToInt16),
                new TypeMapping<short?>(typeof(short), (obj) => {return (obj is DBNull || obj == null) ? null : Convert.ToInt16(obj); }),
                new TypeMapping<int>(typeof(int), Convert.ToInt32),
                new TypeMapping<int?>(typeof(int?), (obj) => {return (obj is DBNull || obj == null) ? null : Convert.ToInt32(obj); }),
                new TypeMapping<long>(typeof(long), Convert.ToInt64),
                new TypeMapping<long?>(typeof(long?), (obj) => {return (obj is DBNull || obj == null) ? null : Convert.ToInt64(obj); }),
                new TypeMapping<float>(typeof(float), (obj) => (float)Convert.ToDecimal(obj)),
                new TypeMapping<double>(typeof(double), Convert.ToDouble),
                new TypeMapping<DateTime>(typeof(DateTime), Convert.ToDateTime),
                new TypeMapping<DateTime?>(typeof(DateTime?), (value) => { return (value is DBNull || value == null) ? null : Convert.ToDateTime(value); }),
                new TypeMapping<Guid>(typeof(Guid), (obj) => Guid.Parse(Convert.ToString(obj) ?? ""))
            };
        }

		public static TypeMapping<T> GetMapping<T>()
		{
			var mapping = (TypeMapping<T>)TypeMappings.FirstOrDefault(m => m.TargetType == typeof(T));
            if (mapping == null)
                throw new KeyNotFoundException($"Type mapping of type '{typeof(T).Name}' not found in registered type maps.");
            return mapping;
        }

        public static ITypeMapping GetMapping(string name)
        {
            var mapping = TypeMappings.FirstOrDefault(m => m.Name == name);
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
