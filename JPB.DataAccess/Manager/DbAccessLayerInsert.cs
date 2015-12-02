using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        private void DbAccessLayer_Insert()
        {
            RangerInsertPation = 25;
        }

        /// <summary>
        /// Insert a <param name="entry"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void Insert(Type type, object entry)
        {
            Insert(type, entry, Database);
        }

        /// <summary>
        /// Insert a <param name="entry"></param>
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void Insert<T>(T entry)
        {
            Insert(typeof(T), entry);
        }

        /// <summary>
        /// Insert a <param name="entry"></param> ,then selectes this entry based on the last inserted ID and creates a new model
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InsertWithSelect<T>(T entry)
        {
            return InsertWithSelect(entry, Database);
        }

        /// <summary>
        /// get the size of the Partition of the singel InsertStatements
        /// </summary>
        public static uint RangerInsertPation { get; set; }

        /// <summary>
        /// Creates AutoStatements in the size of RangerInsertPation
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void InsertRange<T>(IEnumerable<T> entry)
        {
            Database.RunInTransaction(c =>
            {
                var insertRangeCommand = CreateInsertRangeCommand(entry.ToArray(), c);
                //TODO 
                uint curr = 0;
                foreach (var item in insertRangeCommand)
                {
                    curr += RangerInsertPation;
                    RaiseInsert(entry.Skip(((int)curr)).Take((int)RangerInsertPation), item, c);
                    c.ExecuteNonQuery(item);
                }
            });
        }

        /// <summary>
        /// Creates the Multi Insert statement based on the Ranger property
        /// </summary>
        /// <param name="entrys"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDbCommand[] CreateInsertRangeCommand<T>(T[] entrys, IDatabase db)
        {
            var commands = new List<IDbCommand>();
            IDbCommand insertRange = null;

            uint toke = 0;
            var type = typeof(T);

            for (int i = 0; i < entrys.Count(); i++)
            {
                var singelCommand = _CreateInsert(type, entrys[i], db);

                if (insertRange == null)
                {
                    insertRange = singelCommand;
                    continue;
                }
                else
                {
                    insertRange = db.MergeCommands(insertRange, singelCommand, true);
                }
                toke++;

                if (toke == RangerInsertPation)
                {
                    toke = 0;
                    commands.Add(insertRange);
                    insertRange = null;
                }
            }

            commands.Add(insertRange);
            return commands.ToArray();
        }

        /// <summary>
        /// Creates a single Insert Statement with the propertys of <param name="entry"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCommand _CreateInsert(Type type, object entry, IDatabase db)
        {
            string[] ignore =
                ConfigHelper.GetPropertiesEx(type)
                    .Where(
                        s =>
                            s.CheckForPK() ||
                            s.GetCustomAttributes().Any(e => e is InsertIgnore || e is IgnoreReflectionAttribute))
                    .Select(s => s.Name)
                    .Concat(type.CreateIgnoreList())
                    .ToArray();
            string[] propertyInfos = type.CreatePropertyNamesAndMap(ignore).ToArray();
            string csvprops = type.CreatePropertyCSV(ignore);

            string values = "";
            for (int index = 0; index < propertyInfos.Length; index++)
                values = values + ("@" + index + ",");
            values = values.Remove(values.Length - 1);
            string query = "INSERT INTO " + type.GetTableName() + " ( " + csvprops + " ) VALUES ( " + values + " )";

            string[] orignialProps = type.GetPropertysViaRefection(ignore).ToArray();

            ValidateEntity(entry);
            return db.CreateCommandWithParameterValues(type, query, orignialProps, entry);
        }

        /// <summary>
        /// Creates a single insert statement for a <param name="entry"></param> uses <param name="parameter"></param> if possible
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IDbCommand CreateInsert(Type type, object entry, IDatabase db, params object[] parameter)
        {
            return type.CreateCommandOfClassAttribute<InsertFactoryMethodAttribute>(entry, db, (e, f) => _CreateInsert(type, e, f), parameter);
        }

        /// <summary>
        /// Creates and Executes a Insert statement for a given <param name="entry"></param>
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        public static void Insert<T>(T entry, IDatabase db)
        {
            Insert(typeof(T), entry, db);
            //db.Run(s => { s.ExecuteNonQuery(CreateInsert(typeof(T), entry, s)); });
        }

        /// <summary>
        /// Creates and Executes a Insert statement for a given <param name="entry"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        public static void Insert(Type type, object entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateInsert(type, entry, s)); });
        }

        internal static IDbCommand CreateInsertWithSelectCommand(Type type, object entry, IDatabase db)
        {
            var dbCommand = CreateInsert(type, entry, db);
            return db.MergeCommands(dbCommand, db.GetlastInsertedIdCommand());
        }

        /// <summary>
        /// Creates and Executes a Insert statement for a given <param name="entry"></param>
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static object InsertWithSelect(Type type, object entry, IDatabase db)
        {
            return db.Run(s =>
            {
                var mergeCommands = CreateInsertWithSelectCommand(type, entry, db);
                RaiseInsert(entry, mergeCommands, s);
                return Select(type, s.GetSkalar(mergeCommands), s);
            });
        }

        /// <summary>
        /// Not Connection save
        /// Must be executed inside a Valid Connection
        /// Takes <paramref name="base"/> as base of Connection propertys
        /// Merges the Command text of Both commands sepperated by a space
        /// Creats a new command based on <param name="db"></param> and Adds the Merged Commandtext and all parameter to it
        /// </summary>
        /// <param name="base"></param>
        /// <param name="last"></param>
        /// <param name="autoRename">allows an Automatik renaming of multible Commands</param>
        /// <returns></returns>
        public static IDbCommand ConcatCommands(IDatabase db, IDbCommand @base, IDbCommand last, bool autoRename = false)
        {
            return db.MergeTextToParameters(@base, last, autoRename);
        }

        /// <summary>
        /// Not Connection save
        /// Must be executed inside a Valid Connection
        /// Takes <paramref name="base"/> as base of Connection propertys
        /// Merges the Command text of Both commands sepperated by a space
        /// Creats a new command based on <param name="db"></param> and Adds the Merged Commandtext and all parameter to it
        /// </summary>
        /// <param name="base"></param>
        /// <param name="last"></param>
        /// <param name="autoRename">allows an Automatik renaming of multible Commands</param>
        /// <returns></returns>
        public static IDbCommand InsertCommands(IDatabase db, IDbCommand @base, IDbCommand toInsert, bool autoRename = false)
        {
            throw new NotSupportedException();
            //var mergedCommandText = string.Format(@base.CommandText, toInsert);
            //return db.MergeTextToParameters(mergedCommandText, @base, toInsert, autoRename);
        }     

        /// <summary>
        /// Creates and Executes a Insert statement for a given <param name="entry"></param> and selectes that
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T InsertWithSelect<T>(T entry, IDatabase db)
        {
            return (T)InsertWithSelect(typeof(T), entry, db);
        }
    }
}