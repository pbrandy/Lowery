using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Lowery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lowery30Demo
{
	internal class Module1 : Module
	{
		private static Module1 _this = null;
		public LoweryConnection DB { get; set; }
		public LoweryMap LoweryMap { get; set; }
		public string ToolActiveCondition { get; } = "ToolActiveCondition";

        public Module1()
        {
			DB = new LoweryConnection("C:\\Users\\kyled\\Documents\\ArcGIS\\Projects\\LoweryTest\\LoweryTest.gdb");
			//DB = new LoweryConnection("C:\\Users\\Kyle\\Documents\\ArcGIS\\Projects\\PGE_Test\\PGE_Test.gdb");

			MapViewInitializedEvent.Subscribe(RegisterMap);
		}

        private async void RegisterMap(MapViewEventArgs args)
		{
			LoweryMap = new LoweryMap(args.MapView.Map);
			string jsonData = File.ReadAllText("MapDescription.json");
			LoweryMap.MapDefinition = new LoweryMapDefinition(LoweryMap.Map, jsonData);
			LoweryMap.ValidityCondition = ToolActiveCondition;
			await LoweryMap.RegisterExisting();
		}

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("LoweryDemo_Module");

		#region Overrides
		protected override bool Initialize()
		{
			
			return base.Initialize();
		}

		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
		{
			//TODO - add your business logic
			//return false to ~cancel~ Application close
			return true;
		}

		#endregion Overrides

	}
}
