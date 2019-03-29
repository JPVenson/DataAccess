#region

using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;

#endregion

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
			var query = CreateDeleteQueryFactory(GetClassInfo(typeof(T)), null);
			RaiseDelete(null, query);
			Database.Run(s => { ExecuteGenericCommand(query); });
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry)
		{
			var query = CreateDeleteQueryFactory(GetClassInfo(entry.GetType()), entry);
			RaiseDelete(entry, query);
			Database.Run(s => { ExecuteGenericCommand(query); });
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(object primaryKey)
		{
			Database.Run(s =>
			{
				Query().Delete<T>().Where.PrimaryKey().Is.EqualsTo(primaryKey).ExecuteNonQuery();
			});
		}

		/// <summary>
		///     Creates a Delete statement for the given entry
		/// </summary>
		/// <param name="db">The database.</param>
		/// <param name="classInfo">The class information.</param>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		[Obsolete("Duplicate. Use CreateDelete.", true)]
		public static IDbCommand CreateDeleteSimple(IDatabase db, DbClassInfoCache classInfo, object primaryKey)
		{
			if (primaryKey == null)
			{
				return db.CreateCommand("DELETE FROM " + classInfo.TableName);
			}

			if (classInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("No Primary key on '{0}' was supplyed. Operation is not supported",
				classInfo.Name));
			}
			var proppk = classInfo.PrimaryKeyProperty.DbName;
			var query = "DELETE FROM " + classInfo.TableName + " WHERE " + proppk + " = @0";
			return db.CreateCommandWithParameterValues(query,
				new Tuple<Type, object>(classInfo.PrimaryKeyProperty.PropertyType, primaryKey));
		}

		/// <summary>
		///     Creates a Delete statement for the given entry
		/// </summary>
		/// <param name="db">The database.</param>
		/// <param name="classInfo">The class information.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		public static IDbCommand CreateDelete(IDatabase db, DbClassInfoCache classInfo, object entry = null)
		{
			if (entry == null)
			{
				return db.CreateCommand("DELETE FROM " + classInfo.TableName);
			}

			if (classInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("No Primary key on '{0}' was supplyed. Operation is not supported",
				classInfo.Name));
			}
			var proppk = classInfo.PrimaryKeyProperty.DbName;
			var query = "DELETE FROM " + classInfo.TableName + " WHERE " + proppk + " = @0";
			return db.CreateCommandWithParameterValues(query,
				new Tuple<Type, object>(classInfo.PrimaryKeyProperty.PropertyType, classInfo.PrimaryKeyProperty.Getter.Invoke(entry)));
		}

		/// <summary>
		///     Creates and Executes a Standart SQL delete statement based on the Entry
		///     uses factory Mehtod if availbile
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Delete<T>(T entry, params object[] parameter)
		{
			Database.Run(s =>
			{
				var query = CreateDeleteQueryFactory(GetClassInfo(entry.GetType()), entry, parameter);
				RaiseDelete(entry, query);
				ExecuteGenericCommand(query);
			});
		}
	}
}