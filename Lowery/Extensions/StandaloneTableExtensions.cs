using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Extensions
{
	public static class StandaloneTableExtensions
	{
		public static string? GetParentName(this StandaloneTable layer)
		{
			return (layer.Parent as MapMember)?.Name;
		}
	}
}
