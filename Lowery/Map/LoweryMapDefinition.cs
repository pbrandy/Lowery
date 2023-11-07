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
		private Dictionary<string, GroupLayer> _grouplayers = new Dictionary<string, GroupLayer>();
		private Dictionary<string, DataSource> _dataSources = new Dictionary<string, DataSource>();

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
						_dataSources.Add(definition.Name, new DataSource(definition));
					}
				}

				// Make Group Layers
				JsonArray? groupArray = data["GroupLayers"]?.AsArray();
				if (groupArray != null)
				{
					List<LoweryGroupDefinition> groupList = dataSourceArray.Deserialize<List<LoweryGroupDefinition>>() ?? new();
					groupList.ForEach(definition => { _grouplayers.Add(definition.Name, CreateGroupLayer(definition)); });
				}

				// Feature Layers
				JsonArray? layerArray = data["FeatureLayers"]?.AsArray();
				if (layerArray != null)
				{
					List<LoweryFeatureDefinition> featureList = layerArray.Deserialize<List<LoweryFeatureDefinition>>() ?? new();
					featureList.ForEach(async definition => { 
						if (_dataSources.TryGetValue(definition.DataSource, out var dataSource))
						{
							LoweryFeatureLayer fl = await CreateFeatureLayer(definition, dataSource);
						}
					});
				}

				// Tables 
				JsonArray? tableArray = data["Tables"]?.AsArray();
				if (tableArray != null)
				{
					foreach (JsonNode node in tableArray)
					{
						string dataSource = (string?)node["DataSource"] ?? "";
						if (string.IsNullOrEmpty(dataSource) || !_dataSources.ContainsKey(dataSource))
							continue;
					}
				}
			});
		}

		private GroupLayer CreateGroupLayer(LoweryGroupDefinition definition)
		{
			if (definition.Parent == null)
				return LayerFactory.Instance.CreateGroupLayer(Map, 0, definition.Name);

			_grouplayers.TryGetValue(definition.Parent, out var parent);
			if (parent != null)
				return LayerFactory.Instance.CreateGroupLayer(parent, 0, definition.Name);
			else
				throw new LayerNotFoundException();
		}

		private async Task<LoweryFeatureLayer> CreateFeatureLayer(LoweryFeatureDefinition definition, DataSource dataSource)
		{
			ILayerContainerEdit parent;
			if (definition.Parent != null && _grouplayers.TryGetValue(definition.Parent, out var container))
				parent = container;
			else
				parent = Map;

			LoweryFeatureLayer layer = new LoweryFeatureLayer(
				definition,
				new Uri(Path.Join(dataSource.Path, definition.Path)));
			await layer.CreateAsync(parent);
			return layer;
		}

		private async Task<StandaloneTable> CreateStandaloneTable(LoweryTableDefintion definition, DataSource dataSource)
		{
			IStandaloneTableContainerEdit parent;
			if (definition.Parent != null && _grouplayers.TryGetValue(definition.Parent, out var container))
				parent = container;
			else
				parent = Map;

			return await QueuedTask.Run(() => {
				return StandaloneTableFactory.Instance.CreateStandaloneTable(null, parent);
			});
		}
	}
}
