using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using Lowery;
using Lowery30Demo.Models;
using System;
using System.Threading.Tasks;

namespace Lowery30Demo.Controls
{
	internal class Test : Button
	{
		protected override async void OnClick()
		{
			try
			{
				DatabaseConnectionProperties props = new(EnterpriseDatabaseType.SQLServer)
				{
					AuthenticationMode = AuthenticationMode.OSA,
					Instance = "fwsqlsvr\\sqlexpress",
					Database = "developer_pgelibrary"
				};
				LoweryConnection gdb = new(props);
				var t = gdb.Table("developer_pgelibrary.DBO.batch");
			}
			catch (Exception ex)
			{
			}
		}
	}
}
