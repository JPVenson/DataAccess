using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
	partial class DbAccessLayer
	{
		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry)
		{
			var deleteCommand = typeof (T).CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, Database,
				CreateDelete);
			RaiseDelete(entry, deleteCommand, Database);
			Database.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}

		internal static IDbCommand CreateDelete<T>(T entry, IDatabase db)
		{
			var type = typeof (T);
			var proppk = type.GetPK();
			var query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @0";
			return db.CreateCommandWithParameterValues(query, new[] {entry.GetPK()});
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		///     uses factory Mehtod if availbile
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void Delete<T>(T entry, IDatabase db, params object[] parameter)
		{
			var deleteCommand = typeof (T).CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, db,
				CreateDelete, parameter);
			RaiseDelete(entry, deleteCommand, db);
			db.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}
	}
}