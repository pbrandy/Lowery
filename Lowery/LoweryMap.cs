using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Lowery.Definitions;
using Lowery.Exceptions;
using Lowery.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Version = ArcGIS.Core.Data.Version;

namespace Lowery
{
    public class LoweryMap
    {
        public Map Map { get; set; }
        public LoweryMapDefinition? MapDefinition { get; set; }
        public Dictionary<string, ItemRegistry> Registries { get; set; } = new();
        public string? ValidityCondition { get; set; }
        public bool IsValid { get; set; }

        public LoweryMap(Map map)
        {
            Map = map;
            LayersAddedEvent.Subscribe(LayersAdded);
            LayersRemovedEvent.Subscribe(LayersRemovedAsync);
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
            if (registry == null)
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
            if (!await ValidateCollection(MapDefinition.Definitions["Features"].Cast<LoweryFeatureDefinition>()))
                status = false;
            if (!await ValidateCollection(MapDefinition.Definitions["Tables"].Cast<LoweryTableDefintion>()))
                status = false;

            if (status && !string.IsNullOrEmpty(ValidityCondition))
                FrameworkApplication.State.Activate(ValidityCondition);
            else if (!status && !string.IsNullOrEmpty(ValidityCondition))
                FrameworkApplication.State.Deactivate(ValidityCondition);

            return status;
        }

        private async Task<bool> ValidateCollection<T>(IEnumerable<T>? collection) where T : ILoweryDefinition, IRegisterable
        {
            if (collection == null)
                return true;

            bool collectionValid = true;
            foreach (T definition in collection)
            {
                ILoweryItem? item = Registry(definition.Registry).Retrieve<ILoweryItem>(definition.Name);
                if (item == null)
                    collectionValid = false;
                else if (!await item.ValidateDefinition(definition))
                    collectionValid = false;
            }
            return collectionValid;
        }

        public async Task<(bool, IEnumerable<string>?)> RegisterExisting()
        {
            if (MapDefinition == null)
                return (false, null);

            var allFeatureLayers = Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            var allTables = Map.GetStandaloneTablesAsFlattenedList();
            List<string> missingItems = new();
            await QueuedTask.Run(() =>
            {
                // Layer Registration
                foreach (LoweryFeatureDefinition def in MapDefinition.Definitions["Features"])
                {
                    LoweryFeatureLayer validTable = def.FindValid(allFeatureLayers);
                    if (validTable != null)
                        Registry(def.Registry).Register(def.Name, validTable);
                    else
                        missingItems.Add(def.Name);
                }
                // Table Registration
                foreach (LoweryTableDefintion def in MapDefinition.Definitions["Tables"])
                {
                    LoweryStandaloneTable validTable = def.FindValid(allTables);
                    if (validTable != null)
                        Registry(def.Registry).Register(def.Name, validTable);
                    else
                        missingItems.Add(def.Name);
                }
            });

            return (missingItems.Count == 0, missingItems);
        }

        public async Task<bool> CreateOrRegister()
        {
            if (MapDefinition == null)
                return false;

            //Groups
            var groups = Map.GetLayersAsFlattenedList().OfType<GroupLayer>();
            foreach (var groupDef in MapDefinition.Definitions["Groups"])
            {
                //Check For ExistingCopy in MapDef.Groups
                if (MapDefinition.GroupLayers.ContainsKey(groupDef.Name))
                    continue;
                //Check For Copy in Map
                if (groups.Any(g => g.Name == groupDef.Name))
                {
                    MapDefinition.GroupLayers.TryAdd(groupDef.Name, groups.First(g => g.Name == groupDef.Name));
                    continue;
                }
				//Create
                MapDefinition.GroupLayers.TryAdd(groupDef.Name, await MapDefinition.CreateGroupLayer((LoweryGroupDefinition)groupDef));
            }


            var features = Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            foreach (var featDef in MapDefinition.Definitions["Features"])
            {
                LoweryFeatureDefinition def = (LoweryFeatureDefinition)featDef;
                //See if already registered
                if (Registry(def.Registry).Items.TryGetValue(def.Name, out ILoweryItem registeredLayer))
                    continue;
                //Register copy if in Map
                if (features.Any(l => l.Name == def.Name))
                {
                    LoweryFeatureLayer discoveredLayer = new(def, features.First(l => l.Name == def.Name));
					Registry(def.Registry).Register(def.Name, discoveredLayer);
                    continue;
				}
				//Create
				LoweryFeatureLayer newLayer = await MapDefinition.CreateFeatureLayer(def);
                Registry(def.Registry).Register(def.Name, newLayer);
            }

			var tables = Map.GetStandaloneTablesAsFlattenedList();
			foreach (var tableDef in MapDefinition.Definitions["Tables"])
			{
				LoweryTableDefintion def = (LoweryTableDefintion)tableDef;
				//See if already registered
				if (Registry(def.Registry).Items.TryGetValue(def.Name, out ILoweryItem registeredLayer))
					continue;
				//Register copy if in Map
				if (features.Any(l => l.Name == def.Name))
				{
					LoweryStandaloneTable discoveredTable = new(def, tables.First(l => l.Name == def.Name));
					Registry(def.Registry).Register(def.Name, discoveredTable);
					continue;
				}
				//Create
				LoweryStandaloneTable newLayer = await MapDefinition.CreateStandaloneTable(def);
				Registry(def.Registry).Register(def.Name, newLayer);
			}
			return true;
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

        #region Versioning
        public async Task SwitchToVersion(string targetVersionName, string dataSource, string? registryName)
        {
            ItemRegistry reg = Registry(registryName);
            LoweryFeatureLayer item = reg.RetrieveByDataSource<LoweryFeatureLayer>(dataSource);
            await QueuedTask.Run(() =>
            {
                Geodatabase? geodatabase = item.FeatureLayer.GetFeatureClass().GetDatastore() as Geodatabase;
                if (geodatabase == null || geodatabase.IsVersioningSupported())
                    throw new VersioningNotSupportedException($"Versioning is not supported on layer {item.FeatureLayer.Name}.");

                using VersionManager versionManager = geodatabase.GetVersionManager();
                using Version currentVersion = versionManager.GetCurrentVersion();
                using Version? targetVersion = versionManager.GetVersions().ToList()
                                                  .FirstOrDefault(v => v.GetName().Equals(targetVersionName, StringComparison.CurrentCultureIgnoreCase));

                if (targetVersion == null)
                    throw new VersionNotFoundException($"Version of name {targetVersionName} not found in {geodatabase.GetPath}");
                Map.ChangeVersion(currentVersion, targetVersion);
            });
        }

        public async Task PostToParentVersion(string dataSource, Func<bool> conflictResolution, string? registryName,
            ReconcileOptions? reconcileOptions, PostOptions? postOptions)
        {

            ItemRegistry reg = Registry(registryName);
            LoweryFeatureLayer item = reg.RetrieveByDataSource<LoweryFeatureLayer>(dataSource);
            await QueuedTask.Run(() =>
            {
                Geodatabase? geodatabase = item.FeatureLayer.GetFeatureClass().GetDatastore() as Geodatabase;
                if (geodatabase == null || !geodatabase.IsVersioningSupported())
                    throw new VersioningNotSupportedException($"Versioning is not supported on layer {item.FeatureLayer.Name}.");

                using VersionManager versionManager = geodatabase.GetVersionManager();
                using Version currentVersion = versionManager.GetCurrentVersion();
                using Version parentVersion = currentVersion.GetParent() ??
                throw new VersionNotFoundException("Parent version not found. Either it has been deleted or you are attempting to post the default version.");

                ReconcileOptions reconcileOptions = new ReconcileOptions(parentVersion)
                {
                    ConflictResolutionMethod = ConflictResolutionMethod.Continue,
                    ConflictDetectionType = ConflictDetectionType.ByRow,
                    ConflictResolutionType = ConflictResolutionType.FavorTargetVersion
                };

                PostOptions postOptions = new PostOptions(parentVersion) { ServiceSynchronizationType = ServiceSynchronizationType.Synchronous };

                ReconcileResult reconcileResult = currentVersion.Reconcile(reconcileOptions, postOptions);
                if (reconcileResult.HasConflicts)
                {
                    if (conflictResolution.Invoke())
                        currentVersion.Post(postOptions);
                }
                else
                    currentVersion.Post(postOptions);
            });
        }
        #endregion

        #region Event Methods
        private async void LayersAdded(LayerEventsArgs args)
        {
            if (args.Layers[0].Map.URI != Map?.URI)
                return;

            if (IsValid)
                return;

            foreach (Layer layer in args.Layers)
            {
                foreach (var registry in Registries)
                {
                    if (registry.Value.Items.ContainsKey(layer.Name) && await Validate())
                        registry.Value.IsValid = true;
                }
            }

            if (Registries.Values.All(x => x.IsValid))
                IsValid = true;
            else
                IsValid = false;
        }

        private async void LayersRemovedAsync(LayerEventsArgs args)
        {
            if (args.Layers[0].Map.URI != Map?.URI)
                return;

            foreach (Layer layer in args.Layers)
            {
                foreach (var registry in Registries)
                {
                    if (registry.Value.Items.ContainsKey(layer.Name) && await Validate())
                        IsValid = true;
                }
            }

            if (Registries.Values.All(x => x.IsValid))
                IsValid = true;
            else
                IsValid = false;
        }
        #endregion
    }
}
