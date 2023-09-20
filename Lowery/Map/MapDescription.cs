using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowery
{
    public class MapDescription
    {
        public string Name { get; set; }
        public MapType MapType { get; set; }
        public MapViewingMode ViewingMode { get; set; }
        public Basemap? Basemap { get; set; }

        public MapDescription(string name, MapType mapType = MapType.Map, 
            MapViewingMode viewingMode = MapViewingMode.Map, Basemap? basemap = null)
        {
            Name = name;
            MapType = mapType;
            ViewingMode = viewingMode;
            Basemap = basemap;
        }
    }
}
