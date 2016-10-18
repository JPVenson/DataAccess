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
		public void Delete<T>()
		{
			var deleteCommand = CreateDeleteQueryFactory(this.GetClassInfo(typeof(T)), null);
			RaiseDelete(null, deleteCommand);
			Database.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry)
		{
			var deleteCommand = CreateDeleteQueryFactory(this.GetClassInfo(entry.GetType()), entry);
			RaiseDelete(entry, deleteCommand);
			Database.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}

		/// <summary>
		///		Creates a Delete statement for the given entry
		/// </summary>
		/// <param name="db">The database.</param>
		/// <param name="classInfo">The class information.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		public static IDbCommand CreateDelete(IDatabase db, DbClassInfoCache classInfo, object entry)
		{
			if (entry == null)
			{
				return db.CreateCommand("DELETE FROM " + classInfo.TableName);
			}

			if (classInfo.PrimaryKeyProperty == null)
				throw new NotSupportedException(string.Format("No Primary key on '{0}' was supplyed. Operation is not supported", classInfo.Name));
			var proppk = classInfo.PrimaryKeyProperty.DbName;
			var query = "DELETE FROM " + classInfo.TableName + " WHERE " + proppk + " = @0";
			return db.CreateCommandWithParameterValues(query, new Tuple<Type, object>(classInfo.PrimaryKeyProperty.PropertyType, classInfo.PrimaryKeyProperty.Getter.Invoke(entry)));
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		///     uses factory Mehtod if availbile
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry, params object[] parameter)
		{
			var deleteCommand = CreateDeleteQueryFactory(this.GetClassInfo(entry.GetType()), entry, parameter);
			RaiseDelete(entry, deleteCommand);
			Database.Run(s => { s.ExecuteNonQuery(deleteCommand); });
		}
	}
}