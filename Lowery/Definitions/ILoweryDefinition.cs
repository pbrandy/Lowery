using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Definitions
{
    public interface ILoweryDefinition
    {
        string Name { get; set; }
        string? Parent { get; set; }
        LoweryFieldDefinition[]? MandatoryFields { get; set; }
    }
}
