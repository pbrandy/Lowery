using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    public class RegisteredItem
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Uri Uri { get; set; }
        public LayerCreationParams? LayerParameters { get; set; }
        public IDisplayTable? DisplayTable { get; set; }
        public Dictionary<string, FieldType> MandatoryFields { get; private set; } = new Dictionary<string, FieldType>();

        public RegisteredItem(string name, Type type, Uri uri)
        {
            Name = name;
            Type = type;
            Uri = uri;
        }

        public RegisteredItem(string name, Type type, LayerCreationParams layerParameters)
        {
            Name = name;
            Type = type;
            Uri = layerParameters.Uri;
            LayerParameters = layerParameters;
        }

        public RegisteredItem(string name, MapMember mapMember)
        {
            Name = name;
            Type = mapMember.GetType();
            DisplayTable = mapMember as IDisplayTable;
            Uri = new Uri(mapMember.URI);
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
