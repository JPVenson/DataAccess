/*
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
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        /// <summary>
        ///     get the size of the Partition of the singel InsertStatements
        /// </summary>
        public uint RangerInsertPation { get; set; }

        private void DbAccessLayer_Insert()
        {
            RangerInsertPation = 25;
        }

        /// <summary>
        ///     Insert a
        ///     <paramref name="entry" />
        /// </summary>
        public void Insert(Type type, object entry)
        {
            Insert(type, entry, Database);
        }

        /// <summary>
        ///     Insert a
        ///     <paramref name="entry" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Insert<T>(T entry)
        {
            Insert(typeof(T), entry);
        }

        /// <summary>
        ///     Creates AutoStatements in the size of RangerInsertPation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void InsertRange<T>(IEnumerable<T> entry)
        {
            Database.RunInTransaction(c =>
            {
                var insertRangeCommand = CreateInsertRangeCommand(entry.ToArray());
                //TODO
                uint curr = 0;
                foreach (var query in insertRangeCommand)
                {
                    curr += RangerInsertPation;
                    RaiseInsert(entry.Skip(((int)curr)).Take((int)RangerInsertPation), query);
                    Database.PrepaireRemoteExecution(query);
                    c.ExecuteNonQuery(query);
                }
            });
        }

        /// <summary>
        ///     Creates the Multi Insert statement based on the Ranger property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDbCommand[] CreateInsertRangeCommand<T>(T[] entrys)
        {
            var commands = new List<IDbCommand>();
            IDbCommand insertRange = null;

            uint toke = 0;
            int tokeAll = 0;
            var type = this.GetClassInfo(typeof(T));

            var compiledRange = RangerInsertPation == 0 ? 0.1 * entrys.Length : (int)RangerInsertPation;

            for (var i = 0; i < entrys.Count(); i++)
            {
                var singelCommand = CreateInsertQueryFactory(type, entrys[i]);
                singelCommand = Database.AppendSuffix(singelCommand, "_" + i);

                if (insertRange == null)
                {
                    insertRange = singelCommand;
                    continue;
                }

                insertRange = Database.MergeTextToParameters(insertRange, singelCommand, true, tokeAll, false);
                toke++;
                tokeAll++;

                if (toke >= compiledRange)
                {
                    toke = 0;
                    commands.Add(insertRange);
                    insertRange = null;
                }
            }
            if (insertRange != null)
                commands.Add(insertRange);
            return commands.ToArray();
        }

        /// <summary>
        ///     Creates a single Insert Statement with the propertys of
        ///     <paramref name="entry" />
        /// </summary>
        /// <returns></returns>
        public static IDbCommand CreateInsert(IDatabase db, DbClassInfoCache classInfo, object entry)
        {
            var ignore =
                classInfo
                    .Propertys
                    .Select(s => s.Value)
                    .Where(f => f.PrimaryKeyAttribute != null || f.InsertIgnore)
                    .Select(s => s.DbName)
                    .ToArray();

            var propertyInfos = classInfo.FilterDbSchemaMapping(ignore).ToArray();
            var csvprops = classInfo.CreatePropertyCsv(ignore);
            string query;

            if (string.IsNullOrEmpty(csvprops))
            {
                query = "INSERT INTO " + classInfo.TableName + " DEFAULT VALUES";
            }
            else
            {
                var values = "";
                for (var index = 0; index < propertyInfos.Length; index++)
                    values = values + ("@" + index + ",");
                values = values.Remove(values.Length - 1);
                query = "INSERT INTO " + classInfo.TableName + " ( " + csvprops + " ) VALUES ( " + values + " )";
            }

            var orignialProps = classInfo.GetPropertysViaRefection(ignore).ToArray();
            return db.CreateCommandWithParameterValues(classInfo, query, orignialProps, entry);
        }

        /// <summary>
        ///     Creates a single insert statement for a
        ///     <paramref name="entry" />
        ///     uses
        ///     <paramref name="parameter" />
        ///     if possible
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateInsert(Type type, object entry, params object[] parameter)
        {
            return CreateInsertQueryFactory(this.GetClassInfo(type), entry, parameter);

            //return type.CreateCommandOfClassAttribute<InsertFactoryMethodAttribute>(entry, db,
            //	(e, f) => _CreateInsert(type, e, f), parameter);
        }

        /// <summary>
        ///     Creates and Executes a Insert statement for a given
        ///     <paramref name="entry" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Insert<T>(T entry, IDatabase db)
        {
            Insert(typeof(T), entry, db);
        }

        /// <summary>
        ///     Creates and Executes a Insert statement for a given
        ///     <paramref name="entry" />
        /// </summary>
        public void Insert(Type type, object entry, IDatabase db)
        {
            var query = CreateInsertQueryFactory(this.GetClassInfo(type), entry);
            Database.PrepaireRemoteExecution(query);
            db.Run(s => { s.ExecuteNonQuery(query); });
        }

        /// <summary>
        ///     Creates an insert command with appended LastInsertedIDCommand from the IDatabase interface
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateInsertWithSelectCommand(Type type, object entry)
        {
            var dbCommand = CreateInsertQueryFactory(this.GetClassInfo(type), entry);
            return Database.MergeCommands(dbCommand, Database.GetlastInsertedIdCommand());
        }

        /// <summary>
        ///     Creates and Executes a Insert statement for a given
        ///     <paramref name="type" />
        /// </summary>
        /// <returns></returns>
        public object InsertWithSelect(Type type, object entry)
        {
            return Database.Run(s =>
            {
                var mergeCommands = CreateInsertWithSelectCommand(type, entry);
                RaiseInsert(entry, mergeCommands);
                return Select(type, s.GetSkalar(mergeCommands));
            });
        }

        /// <summary>
        ///     Creates and Executes a Insert statement for a given
        ///     <paramref name="entry" />
        ///     and selectes that
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InsertWithSelect<T>(T entry)
        {
            return (T)InsertWithSelect(typeof(T), entry);
        }
    }
}