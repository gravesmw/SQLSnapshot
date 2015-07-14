using GravesConsultingLLC.SqlSnapshot.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace GravesConsultingLLC.SqlSnapshot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: SqlSnapshot Hostname Database Port");
                    return;
                }

                string Hostname = args[0];
                string Database = args[1];
                
                int Port;
                if (!int.TryParse(args[2], out Port))
                {
                    Console.WriteLine("Invalid format for port");
                    return;
                }

                string ConnectionString =
                    @"Server=" + Hostname + "," + Port.ToString() + ";Database=" + Database + ";Trusted_Connection=True";

                ProcessSnapshots(ConnectionString);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        static void ProcessSnapshots(string ConnectionString)
        {
            using (SqlRepository Repository = new SqlRepository(ConnectionString))
            {
                IEnumerable<Snapshot> SnapshotDbs =
                    Snapshot.GetSnapshots(Repository);

                if (SnapshotDbs.Count() == 0 || !SnapshotDbs.Any<Snapshot>(x => x.Active == true))
                {
                    Snapshot.Intialize(Repository);
                    return;
                }

                Snapshot ActiveSnapshot =
                    SnapshotDbs.FirstOrDefault<Snapshot>(x => x.Active == true);

                Snapshot InactiveSnapshot =
                    SnapshotDbs.FirstOrDefault<Snapshot>(x => x.Active == false);

                InactiveSnapshot.Activate(Repository);
                ActiveSnapshot.Deactivate();

                using(TransactionScope Scope = new TransactionScope())
                {
                    InactiveSnapshot.Track(Repository);
                    ActiveSnapshot.Track(Repository);

                    Scope.Complete();
                }
            }
        }
    }
}
