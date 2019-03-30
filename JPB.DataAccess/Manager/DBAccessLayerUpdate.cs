﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Helper.LocalDb.Scopes;

#endregion

namespace JPB.DataAccess.Manager
{
	public partial class DbAccessLayer
	{
		private void UpdateDbAccessLayer()
		{
		}

		/// <summary>
		///     Will update by using the CreateUpdate function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Update<T>(T entry, IDatabase db)
		{
			db.Run(f =>
			{
				var query = CreateUpdate(entry, f);
				RaiseUpdate(entry, query);
				ExecuteGenericCommand(query);
			});
		}

		/// <summary>
		///     Will Update by using the CreateUpdate function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool Update<T>(T entry, bool checkRowVersion = false)
		{
			if (checkRowVersion)
			{
				return Database.RunInTransaction(s =>
				{
					if (!CheckRowVersion(entry))
					{
						return false;
					}
					Update(entry, Database);
					return true;
				});
			}
			Update(entry, Database);
			return true;
		}

		/// <summary>
		///     Will create a new Object when
		///     T contains a Valid RowVersionAttribute property
		///     AND
		///     RowVersionAttribute property is not equals the DB version
		///     OR
		///     T does not contain any RowVersionAttribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Refresh<T>(T entry)
		{
			//TODO make async
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					var query = CreateSelect(typeof(T), entry.GetPK(Config));
					RaiseUpdate(entry, query);
					return RunSelect<T>(query).FirstOrDefault();
				}
				return entry;
			});
		}

		/// <summary>
		///     Will update all propertys of entry when
		///     T contains a Valid RowVersionAttribute property
		///     AND
		///     RowVersionAttribute property is not equals the DB version
		///     OR
		///     T does not contain any RowVersionAttribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool RefreshKeepObject<T>(T entry)
		{
			//TODO Make async
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					var query = CreateSelect(entry.GetType(), entry.GetPK(Config));
					RaiseUpdate(entry, query);
					var select = RunSelect<T>(query).FirstOrDefault();

					var updated = false;
					DataConverterExtensions.CopyPropertys(entry, select, Config);

					LoadNavigationProps(select);

					return updated;
				}
				return false;
			});
		}


		/// <summary>
		///     Checks the Row version of the local entry and the server on
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>True when the version is Equals, otherwise false</returns>
		private bool CheckRowVersion<T>(T entry)
		{
			var type = GetClassInfo(typeof(T));
			var rowVersion =
				GetClassInfo(entry
						.GetType())
					.RowVersionProperty;
			if (rowVersion == null)
			{
				return false;
			}

			if (type.PrimaryKeyProperty == null)
			{
				throw new InvalidOperationException("This Operation requires and Primary Key attribute on the entity to succeed");
			}

			var rowversionValue = rowVersion.GetConvertedValue(entry) as byte[];
			if (rowversionValue == null && entry.GetPK(Config) !=
			    DataConverterExtensions.GetDefault(type.PrimaryKeyProperty.PropertyType))
			{
				return false;
			}

			var rowVersionprop = type.GetLocalToDbSchemaMapping(rowVersion.PropertyName);
			var staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.TableName + " WHERE " +
			                       type.PrimaryKeyProperty.DbName + " = " + entry.GetPK(Config);

#pragma warning disable 618
			var skalar = Database.GetSkalar(staticRowVersion);
#pragma warning restore 618
			if (skalar == null)
			{
				return false;
			}
			return ((byte[]) skalar).SequenceEqual(rowversionValue);
		}

		private IDbCommand CreateUpdateQueryFactory<T>(T entry, params object[] parameter)
		{
			return CreateUpdateQueryFactory(GetClassInfo(entry.GetType()), entry, parameter);
		}

		/// <summary>
		///     Create a Update statement that will update the entry in the Database
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="classInfo">The class information.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		/// <exception cref="Exception">No primarykey Provied. An autogenerated Update statement could not be created</exception>
		public static IDbCommand CreateUpdate(IDatabase database, DbClassInfoCache classInfo, object entry)
		{
			var pkProperty = classInfo.PrimaryKeyProperty;
			if (pkProperty == null)
			{
				throw new Exception("No primarykey Provied. An autogenerated Update statement could not be created");
			}
			var pk = classInfo.SchemaMappingLocalToDatabase(pkProperty.PropertyName);

			var identityInsert = DbIdentityInsertScope.Current != null;
			var ignore =
				classInfo
					.Propertys
					.Select(f => f.Value)
					.Where(s => (!identityInsert && s.PrimaryKeyAttribute != null) || s.InsertIgnore || s.UpdateIgnore || s.ForginKeyAttribute != null)
					.Select(s => s.DbName)
					.ToArray();
			if (identityInsert)
			{
				DbIdentityInsertScope.Current.EnableIdentityModfiy(classInfo.TableName, database);
			}

			var propertyInfos = classInfo.FilterDbSchemaMapping(ignore).ToArray();

			var sb = new StringBuilder();

			sb.Append("UPDATE ");
			sb.Append(classInfo.TableName);
			sb.Append(" SET ");
			var para = new List<IQueryParameter>();

			for (var index = 0; index < propertyInfos.Length; index++)
			{
				var info = propertyInfos[index];
				var schemaName = classInfo.GetDbToLocalSchemaMapping(info);
				DbPropertyInfoCache property;
				classInfo.Propertys.TryGetValue(schemaName, out property);
				var dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
				para.Add(new QueryParameter(index.ToString(), dataValue, property.PropertyType));
				sb.Append(string.Format(" {0} = @{1} ", info, index));
				if (index + 1 < propertyInfos.Length)
				{
					sb.Append(",");
				}
			}

			para.Add(new QueryParameter("pkValue", pkProperty.Getter.Invoke(entry), pkProperty.PropertyType));

			sb.Append(string.Format("WHERE {0} = @pkValue ", pk));

			return database.CreateCommandWithParameterValues(sb.ToString(), para);
		}

		/// <summary>
		///     Create a Update statement that will update the entry in the Database
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="classInfo">The class information.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		/// <exception cref="Exception">No primarykey Provied. An autogenerated Update statement could not be created</exception>
		public static IDbCommand CreateUpdateSimple(IDatabase database, DbClassInfoCache classInfo, object entry)
		{
			var ignore =
				classInfo
					.Propertys
					.Select(f => f.Value)
					.Where(s => s.PrimaryKeyAttribute != null || s.InsertIgnore || s.UpdateIgnore || s.ForginKeyAttribute != null)
					.Select(s => s.DbName)
					.ToArray();

			var propertyInfos = classInfo.FilterDbSchemaMapping(ignore).ToArray();

			var sb = new StringBuilder();

			sb.Append("UPDATE ");
			sb.Append(classInfo.TableName);
			sb.Append(" SET ");
			var para = new List<IQueryParameter>();

			for (var index = 0; index < propertyInfos.Length; index++)
			{
				var info = propertyInfos[index];
				var schemaName = classInfo.GetDbToLocalSchemaMapping(info);
				DbPropertyInfoCache property;
				classInfo.Propertys.TryGetValue(schemaName, out property);
				var dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
				para.Add(new QueryParameter(index.ToString(), dataValue, property.PropertyType));
				sb.Append(string.Format(" {0} = @{1} ", info, index));
				if (index + 1 < propertyInfos.Length)
				{
					sb.Append(",");
				}
			}

			return database.CreateCommandWithParameterValues(sb.ToString(), para);
		}

		/// <summary>
		///     Will create an Update Statement by using a Factory or Autogenerated statements
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDbCommand CreateUpdate<T>(T entry, IDatabase db)
		{
			return CreateUpdateQueryFactory(entry, db);
		}
	}
}