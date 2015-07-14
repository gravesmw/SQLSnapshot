using System;
using System.Collections.Generic;
using System.Configuration;
using Dapper;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace GravesConsultingLLC.SqlSnapshot.Components
{
    public class SqlRepository : IDisposable
    {
        public SqlConnection Connection;

        private IDbConnection _DBConnection;
        private IDbTransaction _DBTransaction;

        public SqlRepository(string ConnectionString)
        {
            Connection =
                new SqlConnection(ConnectionString);

            _DBConnection = Connection;

            _DBConnection.Open();
        }

        public IEnumerable<T> Get<T>(string Procedure, Dictionary<string, object> Parameters, bool IsProcedure)
        {
            return
                _DBConnection.Query<T>(
                    Procedure,
                    Parameters,
                    commandType: IsProcedure ? CommandType.StoredProcedure : CommandType.Text
            );
        }

        public void Put(string Procedure, Dictionary<string, object> Parameters, bool IsProcedure)
        {
            _DBConnection.Execute(
                Procedure,
                Parameters,
                transaction: _DBTransaction != null ? _DBTransaction : null,
                commandType: IsProcedure ? CommandType.StoredProcedure : CommandType.Text
            );
        }

        public void BeginTransaction()
        {
            _DBTransaction = _DBConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_DBTransaction != null)
            {
                _DBTransaction.Commit();
            }
        }

        public void RollbackTransaction()
        {
            if (_DBTransaction != null)
            {
                _DBTransaction.Rollback();
            }
        }

        public void Dispose()
        {
            if (_DBConnection != null && _DBConnection.State != ConnectionState.Closed)
            {
                _DBConnection.Close();
            }
        }
    }
}
