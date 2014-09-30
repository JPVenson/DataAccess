using System;
using System.Data;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbEventArgs;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        public void Delete<T>(T entry)
        {
            var deleteCommand = CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, this.Database, CreateDelete);
            RaiseKnownDelete(deleteCommand);
            this.Database.Run(s =>
            {
                s.ExecuteNonQuery(deleteCommand);
            });
        }

        internal static IDbCommand CreateDelete<T>(T entry, IDatabase db)
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @0";
            return CreateCommandWithParameterValues(query, db, new object[] { entry.GetPK<T, long>() });
        }

        public static void Delete<T>(T entry, IDatabase db, params object[] parameter)
        {
            var deleteCommand = CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, db, CreateDelete, parameter);
            RaiseUnknownDelete(deleteCommand);
            db.Run(s =>
            {
                s.ExecuteNonQuery(deleteCommand);
            });
        }
    }
}