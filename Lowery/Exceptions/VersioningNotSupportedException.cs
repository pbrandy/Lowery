using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Exceptions
{
	public class VersioningNotSupportedException : Exception
	{
		public VersioningNotSupportedException()
		{
		}

		public VersioningNotSupportedException(string message)
			: base(message)
		{
		}

		public VersioningNotSupportedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
