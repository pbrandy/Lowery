using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    public class ItemRegistry
    {
        public LoweryMap Parent { get; set; }
        public Dictionary<string, ILoweryItem> Items { get; private set; } = new Dictionary<string, ILoweryItem>();
        public bool IsValid { get; set; }
        public bool ValidityRequiresSameVersion { get; set; }

        public ItemRegistry(LoweryMap map)
        {
            Parent = map;
        }

        internal bool TestValidity()
        {
            if (Parent.Map == null)
                return false;

            var layers = Parent.Map.GetLayersAsFlattenedList();
            bool valid = true;
            foreach (var item in Items)
            {
                var matchedLayer = layers.FirstOrDefault(l => l.URI == item.Value.Uri.ToString());
                if (matchedLayer is null)
                    valid = false;
            }
            return valid;
        }

        public LoweryStandaloneTable RegisterTable(string name, LoweryTableDefintion definition, StandaloneTable standaloneTable)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name), "Registered layer name cannot be null, empty, or white space.");

			if (Items.ContainsKey(name))
				throw new InvalidOperationException($"Could not register layer with name '{name}'. An item with that key had " +
					$"already been registered in this registry");

            var newItem = new LoweryStandaloneTable(definition, standaloneTable);
            newItem.Registry = this;
            Items.Add(name, newItem);
            return newItem;
		}

        public LoweryFeatureLayer RegisterLayer(string name, LoweryFeatureDefinition definition, FeatureLayer featureLayer)
        {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name), "Registered layer name cannot be null, empty, or white space.");

			if (Items.ContainsKey(name))
				throw new InvalidOperationException($"Could not register layer with name '{name}'. An item with that key had " +
					$"already been registered in this registry");

            var newItem = new LoweryFeatureLayer(definition, featureLayer);
			newItem.Registry = this;
			Items.Add(name, newItem);
			return newItem;
		}

        public void Remove(string name)
        {
            Items.Remove(name);
        }
    }
}
