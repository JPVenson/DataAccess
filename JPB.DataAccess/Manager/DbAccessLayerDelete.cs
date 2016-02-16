/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
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
			var deleteCommand = CreateDeleteQueryFactory(entry.GetType().GetClassInfo(), entry, Database);
			RaiseDelete(entry, deleteCommand, Database);
			Database.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}

		internal IDbCommand _CreateDelete(DbClassInfoCache classInfo, object entry, IDatabase db)
		{
			if (classInfo.PrimaryKeyProperty == null)
				throw new NotSupportedException(string.Format("No Primary key on '{0}' was supplyed. Operation is not supported", classInfo.ClassName));

			var proppk = classInfo.PrimaryKeyProperty.DbName;
			var query = "DELETE FROM " + classInfo.TableName + " WHERE " + proppk + " = @0";
			return db.CreateCommandWithParameterValues(query, new Tuple<Type, object>(classInfo.PrimaryKeyProperty.PropertyType, classInfo.PrimaryKeyProperty.Getter.Invoke(entry)));
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		///     uses factory Mehtod if availbile
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry, IDatabase db, params object[] parameter)
		{
			var deleteCommand = CreateDeleteQueryFactory(entry.GetType().GetClassInfo(), entry, db, parameter);
			RaiseDelete(entry, deleteCommand, db);
			db.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}
	}
}