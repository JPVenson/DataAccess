using System;
using System.Data;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbEventArgs;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        /// <summary>
        /// Creates and Executes a Standart SQL delete statement based on the Entry
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void Delete<T>(T entry)
        {
            var deleteCommand = typeof(T).CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, this.Database, CreateDelete);
            RaiseDelete(entry, deleteCommand, Database);
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
            ValidateEntityPk(entry);
            return db.CreateCommandWithParameterValues(query, new object[] { entry.GetPK<T, long>() });
        }

        /// <summary>
        /// Creates and Executes a Standart SQL delete statement based on the Entry 
        /// uses factory Mehtod if availbile 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <param name="parameter"></param>
        public static void Delete<T>(T entry, IDatabase db, params object[] parameter)
        {
            var deleteCommand = typeof(T).CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, db, CreateDelete, parameter);
            RaiseDelete(entry, deleteCommand, db);
            db.Run(s =>
            {
                s.ExecuteNonQuery(deleteCommand);
            });
        }
    }
}