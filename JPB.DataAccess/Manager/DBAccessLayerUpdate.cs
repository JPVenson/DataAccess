using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Config;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryBuilder;

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
		public static void Update<T>(T entry, IDatabase db)
		{
			db.RunInTransaction(s =>
			{
				IDbCommand dbCommand = CreateUpdate(entry, s);
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
		///     T contains a Valid RowVersion property
		///     AND
		///     RowVersion property is not equals the DB version
		///     OR
		///     T does not contain any RowVersion
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Refresh<T>(T entry)
		{
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					IDbCommand query = CreateSelect(typeof (T), s, entry.GetPK<T, object>());
					RaiseUpdate(entry, query, s);
					return RunSelect<T>(query).FirstOrDefault();
				}
				return entry;
			});
		}

		/// <summary>
		///     Will update all propertys of entry when
		///     T contains a Valid RowVersion property
		///     AND
		///     RowVersion property is not equals the DB version
		///     OR
		///     T does not contain any RowVersion
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool RefreshKeepObject<T>(T entry)
		{
			return Database.RunInTransaction(s =>
			{
				if (!CheckRowVersion(entry))
				{
					IDbCommand query = CreateSelect(entry.GetType(), Database, entry.GetPK<T, object>());
					RaiseUpdate(entry, query, s);
					T @select = RunSelect<T>(query).FirstOrDefault();

					bool updated = false;
					CopyPropertys(entry, @select);

					@select.LoadNavigationProps(Database);

					return updated;
				}
				return false;
			});
		}

		internal static bool CopyPropertys(object @base, object newObject)
		{
			bool updated = false;
			IEnumerable<PropertyInfoCache> propertys = @base.GetType().GetClassInfo().PropertyInfoCaches.Select(f => f.Value);
			foreach (PropertyInfoCache propertyInfo in propertys)
			{
				object oldValue = propertyInfo.GetConvertedValue(@base);
				object newValue = propertyInfo.GetConvertedValue(newObject);

				if (newValue == null && oldValue == null ||
				    (oldValue != null && (newValue == null || newValue.Equals(oldValue))))
					continue;

				propertyInfo.Setter.Invoke(@base, newValue);
				updated = true;
			}
			return updated;
		}

		internal static object GetDefault(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		/// <summary>
		///     Checks the Row version of the local entry and the server on
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>True when the version is Equals, otherwise false</returns>
		private bool CheckRowVersion<T>(T entry)
		{
			Type type = typeof (T);
			PropertyInfoCache rowVersion =
				entry
					.GetType()
					.GetClassInfo()
					.PropertyInfoCaches
					.Select(f => f.Value)
					.FirstOrDefault(s => s.AttributeInfoCaches.Any(f => f.Attribute is RowVersionAttribute));
			if (rowVersion != null)
			{
				var rowversionValue = rowVersion.GetConvertedValue(entry) as byte[];
				if (rowversionValue != null || entry.GetPK() == GetDefault(entry.GetPKType()))
				{
					string rowVersionprop = type.GetLocalToDbSchemaMapping(rowVersion.PropertyName);
					string staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.GetTableName() + " WHERE " +
					                          type.GetPK() + " = " + entry.GetPK();

					object skalar = Database.GetSkalar(staticRowVersion);
					if (skalar == null)
						return false;
					return ((byte[]) skalar).SequenceEqual(rowversionValue);
				}
				return false;
			}
			return false;
		}

		private static IDbCommand CreateUpdateQueryFactory<T>(T entry, IDatabase db, params object[] parameter)
		{
			return typeof (T).CheckInstanceForAttriute<T, InsertFactoryMethodAttribute>(entry, db, createUpdate, parameter);
		}

		internal static IDbCommand createUpdate<T>(T entry, IDatabase db)
		{
			Type type = typeof (T);
			ClassInfoCache classInfo = type.GetClassInfo();
			PropertyInfoCache pkProperty = classInfo.PropertyInfoCaches.FirstOrDefault(s => s.Value.IsPrimaryKey).Value;
			if (pkProperty == null)
				throw new Exception("No primarykey Provied. An autogenerated Update statement could not be created");
			string pk = classInfo.SchemaMappingLocalToDatabase(pkProperty.PropertyName);

			string[] ignore =
				classInfo
					.PropertyInfoCaches
					.Select(f => f.Value)
					.Where(s => s.IsPrimaryKey || s.InsertIgnore || s.ForginKeyAttribute != null)
					.Select(s => s.PropertyName)
					.ToArray();

			string[] propertyInfos = DbAccessLayerHelper.FilterDbSchemaMapping<T>(ignore).ToArray();

			var queryBuilder = new QueryBuilder.QueryBuilder(db);
			queryBuilder.QueryD("UPDATE");
			queryBuilder.QueryD(type.GetTableName());
			queryBuilder.QueryD("SET");
			for (int index = 0; index < propertyInfos.Length; index++)
			{
				string info = propertyInfos[index];
				string schemaName = type.GetDbToLocalSchemaMapping(info);
				PropertyInfoCache property;
				classInfo.PropertyInfoCaches.TryGetValue(schemaName, out property);
				object dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
				queryBuilder.QueryQ(string.Format("{0} = @{1}", info, index), new QueryParameter(index.ToString(), dataValue));
				if (index + 1 < propertyInfos.Length)
					queryBuilder.QueryD(",");
			}

			queryBuilder.Where(string.Format("{0} = @pkValue", pk), new {pkValue = entry.GetPK()});

			return queryBuilder.Compile();
		}

		/// <summary>
		///     Will create an Update Statement by using a Factory or Autogenerated statements
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IDbCommand CreateUpdate<T>(T entry, IDatabase db)
		{
			return CreateUpdateQueryFactory(entry, db);
		}
	}
}