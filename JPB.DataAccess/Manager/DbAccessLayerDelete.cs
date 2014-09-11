using System;
using System.Data;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        public void Delete<T>(T entry)
        {
            Delete<T, long>(entry, Database);
        }

        public void Delete<T, E>(T entry)
        {
            Delete<T, E>(entry, Database);
        }

        public static void Delete<T>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof (T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";

            batchRemotingDb.Run(s =>
            {
                IDbCommand cmd = CreateCommand(s, query);
                cmd.Parameters.AddWithValue("@pk", entry.GetPK<T, long>(), s);
                s.ExecuteNonQuery(cmd);
            });
        }

        public static void Delete<T, E>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof (T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";

            batchRemotingDb.Run(s =>
            {
                IDbCommand cmd = CreateCommand(s, query);
                cmd.Parameters.AddWithValue("@pk", entry.GetPK<T, E>(), s);
                s.ExecuteNonQuery(cmd);
            });
        }
    }
}