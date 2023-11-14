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
        public string Uri { get; set; }
		public DataSource DataSource { get; set; }
		public string? Parent { get; set; }
		public ItemRegistry? Registry { get; set; }
		public LayerCreationParams? LayerParameters { get; set; }
        public IDisplayTable? DisplayTable { get; set; }
        public IEnumerable<LoweryFieldDefinition>? MandatoryFields { get; set; }

		public LoweryFeatureLayer(LoweryFeatureDefinition definition, FeatureLayer instance)
        {
            Name = definition.Name;
            Uri = definition.DataSource;
            FeatureLayer = instance;
            DisplayTable = instance;
            MandatoryFields = definition.MandatoryFields;
        }

        public async Task<bool> Validate()
        {
            if (DisplayTable == null) return false;
            if (MandatoryFields?.Count() == 0) return true;

            return await QueuedTask.Run(() =>
            {
                var fieldDescriptions = DisplayTable.GetFieldDescriptions();
                if (MandatoryFields == null)
                    return true;
                foreach (var field in MandatoryFields)
                {
                    var targetDesc = fieldDescriptions.FirstOrDefault(f => f.Name == field.Field);
                    if (targetDesc is null || field.Type != targetDesc.Type)
                        return false;
                }
                return true;
            });
        }

        public async Task<bool> ValidateDefinition(LoweryFeatureDefinition definition)
        {
			if (DisplayTable == null) return false;

            if (Name != definition.Name)
                return false;

            if ((FeatureLayer.Parent as MapMember)?.Name != definition.Parent)
                return false;

			if (!(MandatoryFields == null ^ definition.MandatoryFields == null))
                return false;

			if (MandatoryFields != null && definition.MandatoryFields != null)
            {
                return await QueuedTask.Run(() => {
					var fieldDescriptions = DisplayTable.GetFieldDescriptions();
					foreach (var field in MandatoryFields)
					{
                        bool containsField = definition.MandatoryFields.Contains(field);
						var targetDesc = fieldDescriptions.FirstOrDefault(f => f.Name == field.Field);
						if (targetDesc is null || field.Type != targetDesc.Type)
							return false;
					}
                    return true;
				});
            }
            else
                return true;
		}
    }
}
