using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
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
	public class LoweryMap
	{
		public Map? Map { get; set; }
		public Dictionary<string, ItemRegistry> Registries { get; set; } = new();

		public LoweryMap()
		{

		}

		public LoweryMap(Map map)
		{
			Map = map;
			LayersAddedEvent.Subscribe(LayersAdded);
			LayersRemovedEvent.Subscribe(LayersAdded);
		}

		private void LayersAdded(LayerEventsArgs args)
		{
			if (args.Layers[0].Map.URI != Map?.URI)
				return;

			foreach (Layer layer in args.Layers)
			{
				foreach (var registry in Registries)
				{
					registry.Value.Items.ContainsKey(layer.Name);
				}
			}
		}

		public async Task<Map> Build(MapDescription description)
		{
			return await QueuedTask.Run(() =>
			{
				Map newMap = MapFactory.Instance.CreateMap(description.Name,
					description.MapType, description.ViewingMode, description.Basemap ?? Basemap.ProjectDefault);

				return newMap;
			});
		}

		public Layer? Layer(string name, string? parentName = null)
		{
			if (Map is null)
				return null;

			if (parentName is null)
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == name);
			else
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == name && ((Layer)l.Parent).Name == parentName);
		}

		public GroupLayer? GroupLayer(string name)
		{
			if (Map is null)
				return null;

			return Map.GetLayersAsFlattenedList().OfType<GroupLayer>().FirstOrDefault(g => g.Name == name);
		}

		public Layer? URILayer(string uri, string? parentName = null)
		{
			if (Map is null)
				return null;

			if (parentName is null)
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.URI == uri).FirstOrDefault();
			else
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.URI == uri && ((Layer)l.Parent).Name == parentName).FirstOrDefault();
		}

		public ItemRegistry CreateRegistry(string name)
		{
			ItemRegistry newRegistry = new(this);
			Registries.Add(name, newRegistry);
			return newRegistry;
		}

		public Table Table(string name)
		{
			return QueuedTask.Run(() =>
			{
				return Map.FindStandaloneTables(name)[0].GetTable();
			}).Result;
		}

		public async Task BuildMapFromJSON(string json)
		{
			var data = JsonNode.Parse(json);
			if (data == null)
				throw new NullReferenceException();

			await QueuedTask.Run(async () =>
			{
				// Gather Data Sources
				Dictionary<string, DataSource> dataSources = new Dictionary<string, DataSource>();
				JsonArray? dataSourceArray = data["DataSources"]?.AsArray();
				if (dataSourceArray != null)
				{
					List<LoweryDataSourceDefinition> dslist = dataSourceArray.Deserialize<List<LoweryDataSourceDefinition>>() ?? new();
                    foreach (var definition in dslist)
                    {
                        dataSources.Add(definition.Name, new DataSource(definition));
                    }
                }

				// Make Group Layers
				Dictionary<string, GroupLayer> groups = new();
                JsonArray? groupArray = data["GroupLayers"]?.AsArray();
				if (groupArray != null)
				{
					List<LoweryGroupDefinition> groupList = dataSourceArray.Deserialize<List<LoweryGroupDefinition>>() ?? new();
					groupList.ForEach(definition => { groups.Add(definition.Name, CreateGroupLayer(definition)); });
				}

				// Feature Layers
				JsonArray? layerArray = data["FeatureLayers"]?.AsArray();
				if (layerArray != null)
				{
					foreach (JsonNode node in layerArray)
					{
						string dataSource = (string?)node["DataSource"] ?? "";
						if (string.IsNullOrEmpty(dataSource) || !dataSources.ContainsKey(dataSource))
							continue;
						LoweryFeatureLayer fl = await CreateFeatureLayer(node, dataSources[dataSource]);
					}
				}

				// Tables 
				JsonArray? tableArray = data["Tables"]?.AsArray();
				if (tableArray != null)
				{
					foreach (JsonNode node in tableArray)
					{
						string dataSource = (string?)node["DataSource"] ?? "";
						if (string.IsNullOrEmpty(dataSource) || !dataSources.ContainsKey(dataSource))
							continue;
					}
				}
			});
		}

		private GroupLayer CreateGroupLayer(LoweryGroupDefinition definition)
		{
			if (definition.Parent == null)
				return LayerFactory.Instance.CreateGroupLayer(Map, 0, definition.Name);

            GroupLayer? parent = GroupLayer(definition.Parent);
            if (parent != null)
                return LayerFactory.Instance.CreateGroupLayer(parent, 0, definition.Name);
            else
                throw new LayerNotFoundException();
		}

		private bool RegisterGroupLayer(LoweryGroupDefinition definition)
		{

		}

		private async Task<LoweryFeatureLayer> CreateFeatureLayer(JsonNode node, DataSource dataSource)
		{
			string? parentName = (string?)node?["Parent"];
			ILayerContainerEdit parent;
			if (string.IsNullOrEmpty(parentName) || GroupLayer(parentName) == null)
				parent = Map;
			else
				parent = GroupLayer(parentName);

			LoweryFeatureLayer layer = new LoweryFeatureLayer(
				(string?)node?["Name"],
				new Uri(Path.Join(dataSource.Path, (string?)node?["Path"])));
			await layer.CreateAsync(parent);
			return layer;
		}
	}
}
