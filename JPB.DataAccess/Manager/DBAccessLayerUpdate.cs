﻿/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;

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
			db.RunInTransaction(s =>
			{
				var dbCommand = CreateUpdate(entry, s);
				RaiseUpdate(entry, dbCommand, s);
				s.ExecuteNonQuery(dbCommand);
			});
		}

		/// <summary>
		///     Will Update by using the CreateUpdate function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool Update<T>(T entry, bool checkRowVersion = false)
		{
			return Database.RunInTransaction(s =>
			{
				if (checkRowVersion)
				{
					if (!CheckRowVersion(entry))
						return false;
				}
				Update(entry, Database);
				return true;
			});
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
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					var query = CreateSelect(typeof(T), entry.GetPK());
					RaiseUpdate(entry, query, s);
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
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					var query = CreateSelect(entry.GetType(), entry.GetPK<T>());
					RaiseUpdate(entry, query, s);
					var @select = RunSelect<T>(query).FirstOrDefault();

					var updated = false;
					DataConverterExtensions.CopyPropertys(entry, @select);

					LoadNavigationProps(@select, Database);

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
			var type = typeof(T).GetClassInfo();
			var rowVersion =
				entry
					.GetType()
					.GetClassInfo()
					.RowVersionProperty;
			if (rowVersion != null)
			{
				var rowversionValue = rowVersion.GetConvertedValue(entry) as byte[];
				if (rowversionValue != null || entry.GetPK() == DataConverterExtensions.GetDefault(type.PrimaryKeyProperty.PropertyType))
				{
					var rowVersionprop = type.GetLocalToDbSchemaMapping(rowVersion.PropertyName);
					var staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.TableName + " WHERE " +
											  type.GetPK() + " = " + entry.GetPK();

					var skalar = Database.GetSkalar(staticRowVersion);
					if (skalar == null)
						return false;
					return ((byte[])skalar).SequenceEqual(rowversionValue);
				}
				return false;
			}
			return false;
		}

		private IDbCommand CreateUpdateQueryFactory<T>(T entry, params object[] parameter)
		{
			return CreateUpdateQueryFactory(entry.GetType().GetClassInfo(), entry, parameter);
		}

		internal IDbCommand _CreateUpdate(DbClassInfoCache classInfo, object entry)
		{
			var pkProperty = classInfo.PrimaryKeyProperty;
			if (pkProperty == null)
				throw new Exception("No primarykey Provied. An autogenerated Update statement could not be created");
			var pk = classInfo.SchemaMappingLocalToDatabase(pkProperty.PropertyName);

			var ignore =
				classInfo
					.Propertys
					.Select(f => f.Value)
					.Where(s => s.PrimaryKeyAttribute != null || s.InsertIgnore || s.ForginKeyAttribute != null)
					.Select(s => s.DbName)
					.ToArray();

			var propertyInfos = classInfo.FilterDbSchemaMapping(ignore).ToArray();

			var queryBuilder = new QueryBuilder<IRootQuery>(this);
			queryBuilder.QueryD("UPDATE");
			queryBuilder.QueryD(classInfo.TableName);
			queryBuilder.QueryD("SET");
			for (var index = 0; index < propertyInfos.Length; index++)
			{
				var info = propertyInfos[index];
				var schemaName = classInfo.GetDbToLocalSchemaMapping(info);
				DbPropertyInfoCache property;
				classInfo.Propertys.TryGetValue(schemaName, out property);
				var dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
				queryBuilder.QueryQ(string.Format("{0} = @{1}", info, index), new QueryParameter(index.ToString(), dataValue));
				if (index + 1 < propertyInfos.Length)
					queryBuilder.QueryD(",");
			}

			queryBuilder.ChangeType<IElementProducer>().Where(string.Format("{0} = @pkValue", pk), new { pkValue = pkProperty.Getter.Invoke(entry) });

			return queryBuilder.ContainerObject.Compile();
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