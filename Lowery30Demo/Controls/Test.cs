using ArcGIS.Desktop.Framework.Contracts;
using Lowery;
using Lowery30Demo.Models;
using System.Threading.Tasks;

namespace Lowery30Demo.Controls
{
	internal class Test : Button
	{
		protected override async void OnClick()
		{
			var reg = Module1.Current.LoweryMap.Registry("Main");
			var result = await reg.Retrieve<LoweryStandaloneTable>("Resources").Get<Resource>("objectid = 1");
		}
	}
}
