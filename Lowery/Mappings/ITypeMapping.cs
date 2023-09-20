using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Mappings
{
    public interface ITypeMapping
    {
        string? Name { get; set; }
        Type TargetType { get; set; }
    }
}
