using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
	public interface ILoweryItem
	{
		string Name { get; }
		public Uri Uri { get; set; }
		public string? Parent { get; set; }
        public ItemRegistry? Registry { get; set; }
    }
}
