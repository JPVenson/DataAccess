using System;
using System.Data;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        private void UpdateDbAccessLayer()
        {
        }

        /// <summary>
        /// Will update by using the CreateUpdate function
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        public static void Update<T>(T entry, IDatabase db)
        {
            db.RunInTransaction(s =>
            {
                var dbCommand = CreateUpdate(entry, s);
                RaiseUpdate(entry, dbCommand, s);
                s.ExecuteNonQuery(dbCommand);
            });
        }

        /// <summary>
        /// Will Update by using the CreateUpdate function
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="checkRowVersion"></param>
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
        /// <param name="entry"></param>
        /// <returns></returns>
        public T Refresh<T>(T entry)
        {
            return Database.RunInTransaction(s =>
            {
                if (!CheckRowVersion(entry))
                {
                    var query = CreateSelect(typeof(T), s, entry.GetPK<T, object>());
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
        ///         RowVersion property is not equals the DB version
        ///         OR
        ///         T does not contain any RowVersion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool RefreshKeepObject<T>(T entry)
        {
            return Database.RunInTransaction(s =>
            {
                if (!CheckRowVersion(entry))
                {
                    var query = CreateSelect(entry.GetType(), Database, entry.GetPK<T, object>());
                    RaiseUpdate(entry, query, s);
                    var @select = RunSelect<T>(query).FirstOrDefault();

                    bool updated = false;
                    CopyPropertys(entry, @select);

                    @select.LoadNavigationProps(Database);

                    return updated;
                }
                return false;
            });
        }

        static internal bool CopyPropertys(object @base, object newObject)
        {
            var updated = false;
            var propertys = ConfigHelper.GetPropertiesEx(@base.GetType());
            foreach (var propertyInfo in propertys)
            {
                object oldValue = propertyInfo.GetConvertedValue(@base);
                object newValue = propertyInfo.GetConvertedValue(newObject);

                if (newValue == null && oldValue == null ||
                    (oldValue != null && (newValue == null || newValue.Equals(oldValue))))
                    continue;

                propertyInfo.SetValue(@base, newValue);
                updated = true;
            }
            return updated;
        }

        public static object GetDefault(Type type)
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
        /// <param name="entry"></param>
        /// <returns>True when the version is Equals, otherwise false</returns>
        private bool CheckRowVersion<T>(T entry)
        {
            Type type = typeof(T);
            var rowVersion =
                ConfigHelper.GetPropertiesEx(entry.GetType())
                    .FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is RowVersionAttribute));
            if (rowVersion != null)
            {
                var rowversionValue = rowVersion.GetConvertedValue(entry) as byte[];
                if (rowversionValue != null || entry.GetPK() == GetDefault(entry.GetPKType()))
                {
                    string rowVersionprop = type.MapEntiysPropToSchema(rowVersion.Name);
                    string staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.GetTableName() + " WHERE " +
                                              type.GetPK() + " = " + entry.GetPK();

                    var skalar = this.Database.GetSkalar(staticRowVersion);
                    if (skalar == null)
                        return false;
                    return ((byte[])skalar).SequenceEqual(rowversionValue);
                }
                return false;
            }
            return false;
        }

        private static IDbCommand CreateUpdateQueryFactory<T>(T entry, IDatabase db, params object[] parameter)
        {
            return typeof(T).CheckInstanceForAttriute<T, InsertFactoryMethodAttribute>(entry, db, createUpdate, parameter);
        }

        internal static IDbCommand createUpdate<T>(T entry, IDatabase db)
        {
            Type type = typeof(T);
            string pk = type.GetPK();

            string[] ignore =
                ConfigHelper.GetPropertiesEx(type)
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes().Any(e => e is InsertIgnore))
                    .Select(s => s.Name)
                    .Concat(type.CreateIgnoreList())
                    .ToArray();

            string[] propertyInfos = DbAccessLayerHelper.CreatePropertyNames<T>(ignore).ToArray();

            var queryBuilder = new QueryBuilder.QueryBuilder(db);
            queryBuilder.QueryD("UPDATE");
            queryBuilder.QueryD(type.GetTableName());
            queryBuilder.QueryD("SET");
            for (int index = 0; index < propertyInfos.Length; index++)
            {
                string info = propertyInfos[index];
                var property = type.GetProperty(type.ReMapSchemaToEntiysProp(info));
                object dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
                queryBuilder.QueryQ(string.Format("{0} = @{1}", info, index), new QueryParameter(index.ToString(), dataValue));
                if(index + 1 < propertyInfos.Length)
                    queryBuilder.QueryD(",");
            }

            queryBuilder.Where(string.Format("{0} = @pkValue", pk), new { pkValue = entry.GetPK() });

            return queryBuilder.Compile();
        }

        /// <summary>
        /// Will create an Update Statement by using a Factory or Autogenerated statements
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDbCommand CreateUpdate<T>(T entry, IDatabase db)
        {
            return CreateUpdateQueryFactory(entry, db);
        }
    }
}