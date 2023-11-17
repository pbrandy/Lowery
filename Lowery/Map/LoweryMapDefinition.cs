using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Lowery.Definitions;
using Lowery.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lowery
{
    public class LoweryMapDefinition
    {
        
        Dictionary<string, GroupLayer> GroupLayers = new Dictionary<string, GroupLayer>();
        Dictionary<string, DataSource> DataSources = new Dictionary<string, DataSource>();

        internal Dictionary<string, List<ILoweryDefinition>> Definitions { get; } = new() {
            { "Groups", new List<ILoweryDefinition>() },
            { "Features", new List<ILoweryDefinition>() },
            { "Tables", new List<ILoweryDefinition>() },
        };

        public Map Map { get; set; }

        public string JSON { get; set; }

        public LoweryMapDefinition(Map map, string json)
        {
            Map = map;
            JSON = json;
            ParseJSON();
        }

        private void ParseJSON()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var data = JsonNode.Parse(JSON);
            if (data == null)
                throw new NullReferenceException();

            // Data Sources
            JsonArray? dataSourceArray = data["DataSources"]?.AsArray();
            if (dataSourceArray != null)
            {
                List<LoweryDataSourceDefinition> dslist = dataSourceArray.Deserialize<List<LoweryDataSourceDefinition>>() ?? new();
                foreach (var definition in dslist)
                {
                    DataSources.Add(definition.Name, new DataSource(definition));
                }
            }

            // Groups
            JsonArray? groupArray = data["GroupLayers"]?.AsArray();
            if (groupArray != null)
                Definitions["Groups"] = groupArray.Deserialize<List<LoweryGroupDefinition>>(options)?.Cast<ILoweryDefinition>().ToList() ?? new();

            // Feature Layers
            JsonArray? layerArray = data["FeatureLayers"]?.AsArray();
            if (layerArray != null)
                Definitions["Features"] = layerArray.Deserialize<List<LoweryFeatureDefinition>>(options)?.Cast<ILoweryDefinition>().ToList() ?? new();

            // Tables 
            JsonArray? tableArray = data["Tables"]?.AsArray();
            if (tableArray != null)
                Definitions["Tables"] = tableArray.Deserialize<List<LoweryTableDefintion>>(options)?.Cast<ILoweryDefinition>().ToList() ?? new();
        }

        public async Task BuildMapFromJSON(string json)
        {
            var data = JsonNode.Parse(json);
            if (data == null)
                throw new NullReferenceException();

            await QueuedTask.Run(() =>
            {
                Definitions["Groups"].ForEach(async definition => { GroupLayers.Add(definition.Name, await CreateGroupLayer((LoweryGroupDefinition)definition)); });

                Definitions["Features"].ForEach(async definition  =>
                {
                    LoweryFeatureDefinition castedDef = (LoweryFeatureDefinition)definition;
                    if (DataSources.TryGetValue(castedDef.DataSource, out var dataSource))
                        await CreateFeatureLayer(castedDef, dataSource);
                });

                Definitions["Tables"].ForEach(async definition =>
                {
                    LoweryTableDefintion castedDef = (LoweryTableDefintion)definition;
                    if (DataSources.TryGetValue(castedDef.DataSource, out var dataSource))
                        await CreateStandaloneTable(castedDef, dataSource);
                });
            });
        }

        public async Task Create(string itemName)
        {
            ILoweryDefinition def = Definitions.Values.SelectMany(x => x).ToList().First(d => d.Name == itemName);
            switch(def)
            {
                case LoweryGroupDefinition group:
                    await CreateGroupLayer(group);
                    break;
                case LoweryFeatureDefinition feature:
                    DataSources.TryGetValue(feature.DataSource, out var datasource);
                    if (datasource != null)
						await CreateFeatureLayer(feature, datasource);
                    break;
                case LoweryTableDefintion table:
                    DataSources.TryGetValue(table.DataSource, out var tableSource);
                    if (tableSource != null)
						await CreateStandaloneTable(table, tableSource);
                    break;
            }
        }

        private async Task<GroupLayer> CreateGroupLayer(LoweryGroupDefinition definition)
        {
            ILayerContainerEdit parent;
            if (definition.Parent != null && GroupLayers.TryGetValue(definition.Parent, out var container))
                parent = container;
            else
                parent = Map;

            if (parent != null)
                return await QueuedTask.Run(() => { return LayerFactory.Instance.CreateGroupLayer(parent, 0, definition.Name); });
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

            FeatureLayer featureLayer = (FeatureLayer)await QueuedTask.Run(async () =>
            {
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

            StandaloneTable standaloneTable = await QueuedTask.Run(() =>
            {
                return StandaloneTableFactory.Instance.CreateStandaloneTable(null, parent);
            });

            LoweryStandaloneTable table = new LoweryStandaloneTable(definition, standaloneTable);
            return table;
        }
    }
}
