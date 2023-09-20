using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class Related : Attribute
    {
        public string RelationshipClass { get; private set; }

        /// <summary>
        /// Designates this data as related to the origin.
        /// </summary>
        /// <param name="relationshipClass"></param>
        public Related(string relationshipClass)
        {
            RelationshipClass = relationshipClass;
        }
    }
}
