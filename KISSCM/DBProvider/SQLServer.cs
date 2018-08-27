using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;



namespace KISS.DBProvider
{
    public class SQLServer:IDBProvider
    {
        Properties _props;
        public SQLServer(Properties props)
        {
            _props = props;
        }

        public int VersionNo()
        {
            int currVersion = 0;
            using (SqlConnection conn = new SqlConnection(_props.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(String.Format("SELECT MAX(id) FROM {0}", _props.VersionTable));
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    object obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value)
                        currVersion = 0;
                    else
                        currVersion = Convert.ToInt32(obj);
                }
                catch (Exception)
                {
                    SqlCommand createStructure = new SqlCommand(this.GetVersionTableSchema());
                    createStructure.Connection = conn;
                    createStructure.ExecuteNonQuery();
                    currVersion = 0;
                }
                finally
                {
                    conn.Close();
                }
            }
            return currVersion;
        }

        public void ExecuteScriptAndUpdateVersionTable(int versionNo, System.IO.FileInfo fi)
        {
            bool success = true;
            Exception exc = null;
            string scriptText = null;
            if (string.IsNullOrWhiteSpace(_props.Encoding))
                scriptText = File.ReadAllText(fi.FullName);
            else
                scriptText = File.ReadAllText(fi.FullName, ArgumentHelper.GetEncoding(_props.Encoding));
            using (SqlConnection sqlConnection = new SqlConnection(_props.ConnectionString))
            {
                Microsoft.SqlServer.Management.Common.ServerConnection svrConnection = new Microsoft.SqlServer.Management.Common.ServerConnection(sqlConnection);
                Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(svrConnection);
                server.ConnectionContext.BeginTransaction();
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(scriptText);
                }
                catch (Exception ex)
                {
                    success = false;
                    exc = ex;
                    server.ConnectionContext.RollBackTransaction();
                }
                if (success)
                {
                    server.ConnectionContext.CommitTransaction();
                    this.UpdateVersion(versionNo, scriptText);
                }
            }
            if (!success)
                throw exc;
        }

        private void UpdateVersion(int versionID, string scriptText)
        {
            using (SqlConnection conn = new SqlConnection(_props.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(String.Format(@"INSERT INTO {0}(id,commited_dt,commited_sql)
                VALUES(@id,@commited_dt,@sql_commited)", _props.VersionTable));
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", versionID));
                cmd.Parameters.Add(new SqlParameter("@commited_dt", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("@sql_commited", scriptText));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public string GetVersionTableSchema()
        {
             return String.Format(@"CREATE TABLE {0}(
                id   INT PRIMARY KEY,
                commited_dt  DATETIME NOT NULL,
                commited_sql    VARCHAR(MAX)
            )",_props.VersionTable);
        }
    }
}
