using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public interface ILoweryDefinition
	{
		string Name { get; }
		string? Parent { get; }

	}
}
