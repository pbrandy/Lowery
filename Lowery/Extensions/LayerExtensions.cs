using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery.Extensions
{
	public static class LayerExtensions
	{
		public static string? GetParentName(this Layer layer)
		{
			return (layer.Parent as MapMember)?.Name;
		}
	}
}
