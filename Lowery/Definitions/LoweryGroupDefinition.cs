using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Definitions
{
    public class LoweryGroupDefinition : ILoweryDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Parent { get; set; }
    }
}
