using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
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
            return QueuedTask.Run(() => {
                return Map.FindStandaloneTables(name)[0].GetTable();
            }).Result;
        }

        public async Task BuildMapFromJSON(string json)
        {
            var data = JsonNode.Parse(json);
            Map map = MapView.Active.Map;
            await QueuedTask.Run(() => {
                // Make Group Layers
                JsonArray list = data["GroupLayers"].AsArray();
                Dictionary<string, GroupLayer> groups = new();
                for (int i = 0; i < list.Count; i++)
                {
                    JsonNode? groupLayer = list[i];
                    string? parentName = (string?)groupLayer?["Parent"];
                    if (parentName == null)
                        LayerFactory.Instance.CreateGroupLayer(Map, 0, (string)groupLayer["Name"]);
                    else
                    {
                        GroupLayer? parent = GroupLayer(parentName);
                        if (parent != null)
                            LayerFactory.Instance.CreateGroupLayer(parent, 0, (string)groupLayer["Name"]);
                    }
                }
                // Feature Layers
                // Tables 
            });
        }
    }
}
