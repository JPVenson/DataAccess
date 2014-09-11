using System;
using System.Data;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        private void UpdateDbAccessLayer()
        {

        }

        public static void Update<T>(T entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateUpdate(entry, s)); });
        }

        public void Update<T>(T entry)
        {
            Update(entry, Database);
        }

        /// <summary>
        /// Will create a new Object when
        /// T contains a Valid RowVersion property
        /// AND 
        /// RowVersion property is not equals the DB version
        /// OR
        /// T does not contain any RowVersion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <returns></returns>
        public T Refresh<T>(T entry) 
        {
            if (!CheckRowVersion(entry))
            {
                return Select<T>(entry.GetPK<T, long>(), Database);
            }
            return entry;
        }

        /// <summary>
        /// Will update all propertys of entry when
        /// T contains a Valid RowVersion property
        /// AND
        /// RowVersion property is not equals the DB version
        /// OR
        /// T does not contain any RowVersion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool RefreshKeepObject<T>(T entry)
        {
            if (!CheckRowVersion(entry))
            {
                var @select = Select<T>(entry.GetPK<T, long>(), Database);
                var updated = false;
                var propertys = typeof(T).GetProperties();
                foreach (var propertyInfo in propertys)
                {
                    var oldValue = propertyInfo.GetValue(entry);
                    var newValue = propertyInfo.GetValue(@select);

                    if (newValue == null && oldValue == null || (oldValue != null && (newValue == null || newValue.Equals(oldValue)))) 
                        continue;

                    propertyInfo.SetValue(@select, newValue);
                    updated = true;
                }

                return updated;
            }
            return false;
        }

        /// <summary>
        /// Checks the Row version of the local entry and the server on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <returns>True when the version is Equals, otherwise false</returns>
        private bool CheckRowVersion<T>(T entry)
        {
            var type = typeof(T);
            var rowVersion =
               entry.GetType()
                   .GetProperties()
                   .FirstOrDefault(s => s.GetCustomAttributes().Any(e => e is RowVersionAttribute));
            if (rowVersion != null)
            {
                var rowversionValue = rowVersion.GetValue(entry) as byte[];
                if (rowversionValue != null || entry.GetPK() == 0)
                {
                    var rowVersionprop = type.MapEntiysPropToSchema(rowVersion.Name);
                    var staticRowVersion = "SELECT " + rowVersionprop + " FROM " + type.GetTableName() + " WHERE " + type.GetPK() + " = " + entry.GetPK();
                    var scalarValue = RunPrimetivSelect<byte[]>(staticRowVersion).FirstOrDefault();
                    return scalarValue != null && scalarValue == rowversionValue;
                }
                return false;
            }
            return false;
        }

        private static IDbCommand CreateUpdateQueryFactory<T>(T entry, IDatabase batchRemotingDb)
        {
            //if (type.GetInterface("IQuerySelectFactory") != null)
            //{
            //    var instance = Activator.CreateInstance(type) as IQuerySelectFactory;
            //    if (instance != null)
            //    {
            //        var queryFactoryResult = instance.CreateSelect();
            //        if (queryFactoryResult.Parameters.Any())
            //        {
            //            return CreateCommandWithParameterValues(queryFactoryResult.Query, batchRemotingDb,
            //                queryFactoryResult.Parameters);
            //        }
            //        {
            //            return CreateCommand(batchRemotingDb, queryFactoryResult.Query);
            //        }
            //    }
            //}

            var type = entry.GetType();

            //try to get a Factory mehtod
            var methods =
                type.GetMethods()
                    .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is UpdateFactoryMethodAttribute));
            if (methods != null)
            {
                //must be public static
                if (!methods.IsStatic)
                {
                    var invoke = methods.Invoke(entry, null);
                    if (invoke != null)
                    {
                        if (invoke is string && !string.IsNullOrEmpty(invoke as string))
                        {
                            return CreateCommand(batchRemotingDb, invoke as string);
                        }
                        if (invoke is IQueryFactoryResult)
                        {
                            var result = invoke as IQueryFactoryResult;
                            return CreateCommandWithParameterValues(result.Query, batchRemotingDb, result.Parameters);
                        }
                    }
                }
            }

            //screw that. Generate a select self!
            return createUpdate(entry, batchRemotingDb);
        }

        internal static IDbCommand createUpdate<T>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof(T);
            string pk = type.GetPK();

            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes(false).Any(e => e is InsertIgnore))
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

            return CreateCommandWithParameterValues(query, propertyInfos, entry, batchRemotingDb);
        }

        public static IDbCommand CreateUpdate<T>(T entry, IDatabase batchRemotingDb)
        {
            return CreateUpdateQueryFactory(entry, batchRemotingDb);
        }
    }
}