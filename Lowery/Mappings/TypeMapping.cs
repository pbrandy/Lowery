using Lowery.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mapping
{
    public class TypeMapping<T> : TypeMappingBase, ITypeMapping
    {
        public new string Name { get; set; }
        public new Type TargetType { get; set; }
        public Func<object, T> ConvertFromDatabase { get; set; }
        public new Func<object, object> ConvertToDatabase { get; set; }

        public TypeMapping(Type targetType, Func<object, T> conversionFromDatabase)
        {
            TargetType = targetType;
            ConvertFromDatabase = conversionFromDatabase;
        }

        public TypeMapping(string name, Type targetType, Func<object, T> conversionFromDatabase)
            : this (targetType, conversionFromDatabase)
        {
            Name = name;
        }

        public TypeMapping(string name, Type targetType, Func<object, T> conversionFromDatabase, Func<object, object> conversionToDatabase)
            : this(name, targetType, conversionFromDatabase)
		{
			ConvertFromDatabase = conversionFromDatabase;
            ConvertToDatabase = conversionToDatabase;
		}

        public T Execute(object obj)
        {
            var rv = ConvertFromDatabase.Invoke(obj);
            return rv;
        }

		public object ConvertBack(object obj)
		{
			return ConvertToDatabase.Invoke(obj);
		}
    }

	public class TypeMapping : ITypeMapping
	{
		public string Name { get; set; }
		public Type TargetType { get; set; }
		public Func<object, object> ConvertFromDatabase { get; set; }
		public Func<object, object> ConvertToDatabase { get; set; }

		public TypeMapping(Type targetType, Func<object, object> conversionFromDatabase)
		{
			TargetType = targetType;
			ConvertFromDatabase = conversionFromDatabase;
		}

		public TypeMapping(string name, Type targetType, Func<object, object> conversionFromDatabase)
			: this(targetType, conversionFromDatabase)
		{
			Name = name;
		}

		public TypeMapping(string name, Type targetType, Func<object, object> conversionFromDatabase, Func<object, object> conversionToDatabase)
			: this(name, targetType, conversionFromDatabase)
		{
			ConvertFromDatabase = conversionFromDatabase;
			ConvertToDatabase = conversionToDatabase;
		}

		public object Execute(object obj)
		{
			var rv = ConvertFromDatabase.Invoke(obj);
			return rv;
		}
	}
}
