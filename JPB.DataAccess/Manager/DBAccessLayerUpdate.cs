using System;
using System.Data;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        private void UpdateDbAccessLayer()
        {
        }

        public static void Update<T>(T entry, IDatabase db)
        {
            db.RunInTransaction(s =>
            {
                var dbCommand = CreateUpdate(entry, s);
                RaiseUnknwonUpdate(dbCommand);
                s.ExecuteNonQuery(dbCommand);
            });
        }

        public bool Update<T>(T entry, bool checkRowVersion = false)
        {
            if (checkRowVersion)
            {
                if (!CheckRowVersion(entry))
                    return false;
            }
            Update(entry, Database);
            return true;
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
                    var query = CreateSelect(typeof(T), s, entry.GetPK<T, long>());
                    RaiseKnownUpdate(query);
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
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool RefreshKeepObject<T>(T entry)
        {
            if (!CheckRowVersion(entry))
            {
                var query = CreateSelect(typeof(T), Database, entry.GetPK<T, long>());
                RaiseKnownUpdate(query);
                var @select = Select<T>(query).FirstOrDefault();

                bool updated = false;
                PropertyInfo[] propertys = typeof(T).GetProperties();
                foreach (PropertyInfo propertyInfo in propertys)
                {
                    object oldValue = propertyInfo.GetConvertedValue(entry);
                    object newValue = propertyInfo.GetConvertedValue(@select);

                    if (newValue == null && oldValue == null ||
                        (oldValue != null && (newValue == null || newValue.Equals(oldValue))))
                        continue;

                    propertyInfo.SetValue(@select, newValue);
                    updated = true;
                }

                @select.LoadNavigationProps(Database);

                return updated;
            }
            return false;
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
            PropertyInfo rowVersion =
                entry.GetType()
                    .GetProperties()
                    .FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is RowVersionAttribute));
            if (rowVersion != null)
            {
                var rowversionValue = rowVersion.GetConvertedValue(entry) as byte[];
                if (rowversionValue != null || entry.GetPK() == 0)
                {
                    string rowVersionprop = type.MapEntiysPropToSchema(rowVersion.Name);
                    string staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.GetTableName() + " WHERE " +
                                              type.GetPK() + " = " + entry.GetPK();
                    byte[] scalarValue = RunPrimetivSelect<byte[]>(staticRowVersion).FirstOrDefault();
                    return scalarValue != null && scalarValue.SequenceEqual(rowversionValue);
                }
                return false;
            }
            return false;
        }

        private static IDbCommand CreateUpdateQueryFactory<T>(T entry, IDatabase db, params object[] parameter)
        {
            return CheckInstanceForAttriute<T, InsertFactoryMethodAttribute>(entry, db, createUpdate, parameter);
        }

        internal static IDbCommand createUpdate<T>(T entry, IDatabase db)
        {
            Type type = typeof(T);
            string pk = type.GetPK();

            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes().Any(e => e is InsertIgnore))
                    .Select(s => s.Name)
                    .Concat(CreateIgnoreList(type))
                    .ToArray();

            string[] propertyInfos = CreatePropertyNames<T>(ignore).ToArray();

            string prop = " SET ";
            for (int index = 0; index < propertyInfos.Length; index++)
            {
                string info = propertyInfos[index];
                prop = prop + (info + " = @" + index + ",");
            }

            prop = prop.Remove(prop.Length - 1);

            string query = "UPDATE " + type.GetTableName() + prop + " WHERE " + pk + " = " + entry.GetPK();

            return CreateCommandWithParameterValues(query, propertyInfos, entry, db);
        }

        public static IDbCommand CreateUpdate<T>(T entry, IDatabase db)
        {
            return CreateUpdateQueryFactory(entry, db);
        }
    }
}