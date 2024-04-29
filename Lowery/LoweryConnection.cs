using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Version = ArcGIS.Core.Data.Version;

namespace Lowery
{
    public class LoweryConnection
    {
        public Connector Connector { get; set; }
        private IReadOnlyList<TableDefinition> Tables { get; set; } = new List<TableDefinition>();
        public Geodatabase Geodatabase { get; private set; }
        public Task Initialize { get; set; }

        public LoweryConnection(Uri databaseConnectionFilePath)
        {
            Connector = new DatabaseConnectionFile(databaseConnectionFilePath);
            Initialize = InitializeAsync();
        }

        public LoweryConnection(DatabaseConnectionProperties databaseConnectionProperties)
        {
            Connector = databaseConnectionProperties;
            Initialize = InitializeAsync();
        }

        public LoweryConnection(string fileGeodatabasePath)
        {
            Connector = new FileGeodatabaseConnectionPath(new Uri(fileGeodatabasePath));
            Initialize = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await QueuedTask.Run(() =>
            {
                Geodatabase? gdb = null;
                switch (Connector)
                {
                    case DatabaseConnectionFile _:
                        gdb = new((DatabaseConnectionFile)Connector);
                        break;
                    case DatabaseConnectionProperties _:
                        gdb = new((DatabaseConnectionProperties)Connector);
                        break;
                    case FileGeodatabaseConnectionPath _:
                        gdb = new((FileGeodatabaseConnectionPath)Connector);
                        break;
                    case MobileGeodatabaseConnectionPath _:
                        gdb = new((MobileGeodatabaseConnectionPath)Connector);
                        break;
                    case MemoryConnectionProperties _:
                        gdb = new((MemoryConnectionProperties)Connector);
                        break;
                }

                if (gdb == null)
                    return;
                Tables = gdb.GetDefinitions<TableDefinition>()
                ?? new List<TableDefinition>();
                Geodatabase = gdb;
            });
        }

        private async Task<Version?> SetConnectionToVersion(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await QueuedTask.Run(() =>
            {
                if (!Geodatabase.IsVersioningSupported())
                    throw new InvalidOperationException($"Geodatabase at '{Geodatabase.GetPath()}' does not support versioning.");

                using (VersionManager vm = Geodatabase.GetVersionManager())
                {
                    var version = vm.GetVersion(name);
                    if (version != null)
                    {
                        version.Connect();
                        return version;
                    }
                    return  null;
                }
            });
        }

        public Table Table(string tableName)
        {
            return QueuedTask.Run(() =>
            {
                return Geodatabase.OpenDataset<Table>(tableName);
            }).Result;
        }

        public RelationshipClass Relation(string relationshipClassName)
        {
            return QueuedTask.Run(() =>
            {
                return Geodatabase.OpenDataset<RelationshipClass>(relationshipClassName);
            }).Result;
        }
    }
}
