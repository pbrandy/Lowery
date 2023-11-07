using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Lowery.Exceptions;
using System;
using System.Collections;
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
		public Map Map { get; set; }
		public Dictionary<string, ItemRegistry> Registries { get; set; } = new();

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

		public ItemRegistry Registry(string name)
		{
			bool exists = Registries.TryGetValue(name, out var registry);
			if (!exists)
			{
				registry = new(this);
				Registries.Add(name, registry);
			}
			return registry;
		}

		#region Accessing
		public GroupLayer? GroupLayer(string name)
		{
			if (Map is null)
				return null;

			return Map.GetLayersAsFlattenedList().OfType<GroupLayer>().FirstOrDefault(g => g.Name == name);
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

		public Layer? URILayer(string uri, string? parentName = null)
		{
			if (Map is null)
				return null;

			if (parentName is null)
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.URI == uri).FirstOrDefault();
			else
				return Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.URI == uri && ((Layer)l.Parent).Name == parentName).FirstOrDefault();
		}

		public Table? Table(string name, string? parentName = null)
		{
			return QueuedTask.Run(() =>
			{
				if (parentName == null)
					return Map.FindStandaloneTables(name)?[0].GetTable();
				else
					return Map.FindStandaloneTables(name).Where(t => ((Layer)t.Parent).Name == parentName).FirstOrDefault()?.GetTable();
			}).Result;
		}

		public IEnumerable<MapMember> SearchForItem(string name, Type type, string? parent = null)
		{
			IEnumerable<MapMember> rv = Array.Empty<MapMember>();
			if (type == typeof(Layer))
			{
				List<Layer> layers;
				if (parent != null)
					layers = Map.FindLayers(name).Where(l => l.GetType() == type && ((MapMember)l.Parent).Name == parent).ToList();
				else
					layers = Map.GetLayersAsFlattenedList().Where(l => l.Name == name && l.GetType() == type).ToList();
				return layers;
			}
			else if (type == typeof(StandaloneTable))
			{
				List<StandaloneTable> tables;
				if (parent != null)
					tables = Map.FindStandaloneTables(name).Where(t => ((MapMember)t.Parent).Name == parent).ToList();
				else
					tables = Map.FindStandaloneTables(name).ToList();
				return tables;
			}
			else
				throw new ArgumentException($"The {type.Name} type is not a valid map member item.");
		}
		#endregion
	}
}
