using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lowery
{
    public class LoweryFeatureLayer : ILoweryItem
    {
        public string Name { get; set; }
        public FeatureLayer FeatureLayer { get; private set; }
        public Uri Uri { get; set; }
		public string? Parent { get; set; }
		public ItemRegistry? Registry { get; set; }
		public LayerCreationParams? LayerParameters { get; set; }
        public IDisplayTable? DisplayTable { get; set; }
        public Dictionary<string, FieldType> MandatoryFields { get; private set; } = new Dictionary<string, FieldType>();

        public LoweryFeatureLayer(string name, Uri uri)
        {
            Name = name;
            Uri = uri;
        }

        public LoweryFeatureLayer(string name, LayerCreationParams layerParameters)
        {
            Name = name;
            Uri = layerParameters.Uri;
            LayerParameters = layerParameters;
        }

        public LoweryFeatureLayer(string name, MapMember mapMember)
        {
            Name = name;
            DisplayTable = mapMember as IDisplayTable;
            Uri = new Uri(mapMember.URI);
        }

        internal LoweryFeatureLayer(string? v)
        {
        }

        public async Task<FeatureLayer> CreateAsync(ILayerContainerEdit container, int index = 0)
        {
			return await QueuedTask.Run(() =>
			{
				FeatureLayer featureLayer = (FeatureLayer)LayerFactory.Instance.CreateLayer(Uri, container, index, Name);
                FeatureLayer = featureLayer;
				return featureLayer;
			});
        }

        public async Task<bool> Validate()
        {
            if (DisplayTable == null) return false;
            if (MandatoryFields.Count == 0) return true;

            return await QueuedTask.Run(() =>
            {
                var fieldDescriptions = DisplayTable.GetFieldDescriptions();
                foreach (var field in MandatoryFields)
                {
                    var targetDesc = fieldDescriptions.FirstOrDefault(f => f.Name == field.Key);
                    if (targetDesc is null || field.Value != targetDesc.Type)
                        return false;
                }
                return true;
            });
        }
    }
}
