using Lowery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoweryDemo.Models
{
    public class ResourceIdentifier
    {
        [PrimaryKey]
        public long ObjectID { get; set; }

        [Related("Resources_ResourceIdentifier")]
        public Resource ResourceID { get; set; }

        public string IDType { get; set; }

        public string Identifier { get; set; }
    }
}
