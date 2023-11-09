using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Lowery.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lowery
{
	public class LoweryMapDefinition
	{
        Dictionary<string, GroupLayer> GroupLayers = new Dictionary<string, GroupLayer>();
        Dictionary<string, DataSource> DataSources = new Dictionary<string, DataSource>();
		internal List<LoweryGroupDefinition> Groups { get; set; } = new();
		internal List<LoweryFeatureDefinition> Features { get; set; } = new();
		internal List<LoweryTableDefintion> Tables { get; set; } = new();

		public Map Map { get; set; }
		
        public string JSON { get; set; }

        public LoweryMapDefinition(Map map, string json)
        {
			Map = map;
			JSON = json;
        }

        public async Task BuildMapFromJSON(string json)
		{
			var data = JsonNode.Parse(json);
			if (data == null)
				throw new NullReferenceException();

			await QueuedTask.Run(async () =>
			{
				// Gather Data Sources
				JsonArray? dataSourceArray = data["DataSources"]?.AsArray();
				if (dataSourceArray != null)
				{
					List<LoweryDataSourceDefinition> dslist = dataSourceArray.Deserialize<List<LoweryDataSourceDefinition>>() ?? new();
					foreach (var definition in dslist)
					{
						DataSources.Add(definition.Name, new DataSource(definition));
					}
				}

				// Make Group Layers
				JsonArray? groupArray = data["GroupLayers"]?.AsArray();
				if (groupArray != null)
				{
					List<LoweryGroupDefinition> groupList = dataSourceArray.Deserialize<List<LoweryGroupDefinition>>() ?? new();
					groupList.ForEach(definition => { GroupLayers.Add(definition.Name, CreateGroupLayer(definition)); });
				}

				// Feature Layers
				JsonArray? layerArray = data["FeatureLayers"]?.AsArray();
				if (layerArray != null)
				{
					List<LoweryFeatureDefinition> featureList = layerArray.Deserialize<List<LoweryFeatureDefinition>>() ?? new();
					Features.AddRange(featureList);
					featureList.ForEach(async definition => { 
						if (DataSources.TryGetValue(definition.DataSource, out var dataSource))
						{
							LoweryFeatureLayer fl = await CreateFeatureLayer(definition, dataSource);
						}
					});
				}

				// Tables 
				JsonArray? tableArray = data["Tables"]?.AsArray();
				if (tableArray != null)
				{
					List<LoweryTableDefintion> tableDefintions = layerArray.Deserialize<List<LoweryTableDefintion>>() ?? new();
					Tables.AddRange(tableDefintions);
					tableDefintions.ForEach(async definition => { 
						if (DataSources.TryGetValue(definition.DataSource, out var dataSource))
						{
							LoweryStandaloneTable standaloneTable = await CreateStandaloneTable(definition, dataSource);
						}
					});
				}
			});
		}

		private GroupLayer CreateGroupLayer(LoweryGroupDefinition definition)
		{
			if (definition.Parent == null)
				return LayerFactory.Instance.CreateGroupLayer(Map, 0, definition.Name);

			GroupLayers.TryGetValue(definition.Parent, out var parent);
			if (parent != null)
				return LayerFactory.Instance.CreateGroupLayer(parent, 0, definition.Name);
			else
				throw new LayerNotFoundException();
		}

		private async Task<LoweryFeatureLayer> CreateFeatureLayer(LoweryFeatureDefinition definition, DataSource dataSource)
		{
			ILayerContainerEdit parent;
			if (definition.Parent != null && GroupLayers.TryGetValue(definition.Parent, out var container))
				parent = container;
			else
				parent = Map;

			FeatureLayer featureLayer = (FeatureLayer)await QueuedTask.Run(async () => {
				return LayerFactory.Instance.CreateLayer(null, parent);
			});
			LoweryFeatureLayer layer = new LoweryFeatureLayer(definition, featureLayer);
			return layer;
		}

		private async Task<LoweryStandaloneTable> CreateStandaloneTable(LoweryTableDefintion definition, DataSource dataSource)
		{
			IStandaloneTableContainerEdit parent;
			if (definition.Parent != null && GroupLayers.TryGetValue(definition.Parent, out var container))
				parent = container;
			else
				parent = Map;

			StandaloneTable standaloneTable = await QueuedTask.Run(() => {
				return StandaloneTableFactory.Instance.CreateStandaloneTable(null, parent);
			});

			LoweryStandaloneTable table = new LoweryStandaloneTable(definition, standaloneTable);
			return table;
		}
	}
}
