using Lowery.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mapping
{
    public class TypeMapping<T> : ITypeMapping
    {
        public string? Name { get; set; }
        public Type TargetType { get; set; }
        public Func<object, T> ConvertFromDatabase { get; set; }
        public Func<T, object>? ConvertToDatabase { get; set; }

        public TypeMapping(Type targetType, Func<object, T> conversionToDB)
        {
            TargetType = targetType;
            ConvertFromDatabase = conversionToDB;
        }

        public TypeMapping(string name, Type targetType, Func<object, T> conversiontToDB)
            : this (targetType, conversiontToDB)
        {
            Name = name;
        }

        public TypeMapping(Type targetType, Func<object, T> conversionToDB, Func<T, object> conversionFromDB)
            : this(targetType, conversionToDB)
        {
            ConvertToDatabase = conversionFromDB;
        }

        public T Execute(object obj)
        {
            var rv = ConvertFromDatabase.Invoke(obj);
            return rv;
        }
    }
}
