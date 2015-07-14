using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;

namespace GravesConsultingLLC.SqlSnapshot.Components
{
    public class SnapshotManagement 
    {
        private Server _Server;
        private SqlRepository _Repository;

        public SnapshotManagement(SqlRepository Repository)
        {
            _Repository = Repository;
            _Server = new Server(
                new ServerConnection(Repository.Connection)
            );
        }

        public void CreateSnaphot(string SnapshotName, string SourceDatabase, string SnapshotLocation)
        {
            Database SrcDatabase = _Server.Databases[SourceDatabase];

            Database SnapshotDatabase = new Database(_Server, SnapshotName);
            SnapshotDatabase.DatabaseSnapshotBaseName = SourceDatabase;

            foreach (FileGroup Group in SrcDatabase.FileGroups)
            {
                SnapshotDatabase.FileGroups.Add(new FileGroup(SnapshotDatabase, Group.Name));
                foreach (DataFile File in Group.Files)
                {
                    SnapshotDatabase.FileGroups[Group.Name].Files.Add(
                        new DataFile(
                            SnapshotDatabase.FileGroups[Group.Name],
                            File.Name,
                            SnapshotLocation + @"\" + SnapshotName + "_" + File.Name + ".ss")
                    );
                }
            }

            DropSnapshot(SnapshotName);

            SnapshotDatabase.Create();

            _Repository.Connection.ChangeDatabase(SourceDatabase);
        }

        public void DropSnapshot(string SnapshotName)
        {
            if (_Server.Databases[SnapshotName] != null)
            {
                _Server.KillAllProcesses(SnapshotName);
                _Server.Databases[SnapshotName].Drop();
            }
        }
       
    }
}
