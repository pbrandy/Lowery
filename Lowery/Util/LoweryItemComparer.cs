using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Util
{
    internal class LoweryItemComparer : EqualityComparer<ILoweryItem>
    {
        public override bool Equals(ILoweryItem? x, ILoweryItem? y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode([DisallowNull] ILoweryItem obj)
        {
            throw new NotImplementedException();
        }
    }
}
