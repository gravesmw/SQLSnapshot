using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace GravesConsultingLLC.SqlSnapshot.Components
{
    public class Snapshot
    {
        private static string _SnapshotTable =
            ConfigurationManager.AppSettings["SnapshotTable"];

        private string _SnapshotLocation =
            ConfigurationManager.AppSettings["SnapshotLocation"];

        public static string[] AllowableName = { "SnapshotA", "SnapshotB" };
        
        public string Name { get; set; }
        public bool Active { get; set; }

        public static IEnumerable<Snapshot> GetSnapshots(SqlRepository Repository)
        {
            string Query = 
                "SELECT Name, Active FROM " + _SnapshotTable;

            return Repository.Get<Snapshot>(Query, null, false);
        }

        public void Activate(SqlRepository Repository)
        {
            SnapshotManagement Management =
                new SnapshotManagement(Repository);

            Management.CreateSnaphot(
                this.Name, 
                Repository.Connection.Database,
                _SnapshotLocation
            );

            this.Active = true;
        }

        public void Deactivate()
        {
            this.Active = false;
        }

        public void Track(SqlRepository Repository)
        {
            string Query = "IF EXISTS(SELECT Name FROM " + _SnapshotTable + " WHERE Name = @Name) BEGIN " +
	            "UPDATE dbo.SnapshotManagement SET Active = @Active, StateChange = GETUTCDATE() " +
	            "WHERE Name = @Name END ELSE BEGIN " +
	            "INSERT INTO dbo.SnapshotManagement(Name, Active, StateChange) " +
	            "VALUES(@Name, @Active, GETUTCDATE()) END";

            Dictionary<string, object> Parameters = new Dictionary<string, object>(){
                { "@Name", this.Name },
                { "@Active", this.Active }
            };

            Repository.Put(Query, Parameters, false);
        }

        public static void Intialize(SqlRepository Repository)
        {
            Snapshot SnapshotA = new Snapshot(){
                Name = "SnapshotA",
                Active = true
            };

            SnapshotA.Activate(Repository);
           
            Snapshot SnapshotB = new Snapshot()
            {
                Name = "SnapshotB",
                Active = false
            };

            using(TransactionScope Scope = new TransactionScope())
            {
                SnapshotA.Track(Repository);
                SnapshotB.Track(Repository);

                Scope.Complete();
            }
        }
    }
}
