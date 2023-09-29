using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Exceptions
{
	public class LayerNotFoundException : Exception
	{
		public LayerNotFoundException()
		{
		}

		public LayerNotFoundException(string message)
			: base(message)
		{
		}

		public LayerNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
