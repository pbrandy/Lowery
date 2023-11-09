using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Lowery.Exceptions;
using Lowery.Extensions;
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
		public LoweryMapDefinition? MapDefinition { get; set; }
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

		public ItemRegistry Registry(string? name = null)
		{
			if (name == null)
				name = "default";
			bool exists = Registries.TryGetValue(name, out var registry);
			if (!exists)
			{
				registry = new(this);
				Registries.Add(name, registry);
			}
			return registry;
		}

		public async Task<bool> Validate()
		{
			if (MapDefinition == null)
				return true;

			bool status = true;
			foreach(var definition in MapDefinition.Features)
			{
				bool validated;
				LoweryFeatureLayer? item = Registry(definition.Registry).Retrieve(definition.Name) as LoweryFeatureLayer;
				if (item == null)
					status = false;
				else
					validated = await item.ValidateDefinition(definition);
			}

			return status;
		}

		public async Task<bool> RegisterExisting()
		{
			if (MapDefinition == null)
				return false;

			var allLayers = Map.GetLayersAsFlattenedList();
			var allTables = Map.GetStandaloneTablesAsFlattenedList();
			List<string> missingItems = new();
			await QueuedTask.Run(() => {
				// Layer Registration
				foreach (var def in MapDefinition.Features)
				{
					IEnumerable<FeatureLayer> matches = allLayers.OfType<FeatureLayer>().Where(l => l.Name == def.Name && l.GetParentName() == def.Parent);
					for (int i = 0; i < def.MandatoryFields?.Length; i++)
					{
						LoweryFieldDefinition field = def.MandatoryFields[i];
						bool validFieldFound = false;
						foreach (var match in matches)
						{
							var descriptions = match.GetFieldDescriptions();
							var validField = descriptions.FirstOrDefault(d => d.Name == field.Field && d.Type == field.Type && d.Alias == field.Alias);
							if (validField != null)
							{
								validFieldFound = true;
								Registry(def.Registry).Register(def.Name, new LoweryFeatureLayer(def, match));
								break;
							}
						}
						if (!validFieldFound)
							missingItems.Add(def.Name);
					}
				}
				// Table Registration
				foreach (var def in MapDefinition.Tables)
				{
					IEnumerable<StandaloneTable> matches = allTables.Where(l => l.Name == def.Name && l.GetParentName() == def.Parent);
					for (int i = 0; i < def.MandatoryFields?.Length; i++)
					{
						LoweryFieldDefinition field = def.MandatoryFields[i];
						bool validFieldFound = false;
						foreach (var match in matches)
						{
							var descriptions = match.GetFieldDescriptions();
							var validField = descriptions.FirstOrDefault(d => d.Name == field.Field && d.Type == field.Type && d.Alias == field.Alias);
							if (validField != null)
							{
								validFieldFound = true;
								Registry(def.Registry).Register(def.Name, new LoweryStandaloneTable(def, match));
								break;
							}
						}
						if (!validFieldFound)
							missingItems.Add(def.Name);
					}
				}
			});
			
			return (missingItems.Count == 0);
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
