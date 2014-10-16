using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        /// <summary>
        /// Insert a <param name="entry"></param>
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void Insert<T>(T entry)
        {
            Insert(entry, Database);
        }

        /// <summary>
        /// Insert a <param name="entry"></param> and then selectes this entry and creates a new model
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InsertWithSelect<T>(T entry)
        {
            return InsertWithSelect(entry, Database);
        }

        /// <summary>
        /// Defines the size of the Partiotion of the singel InsertStatements
        /// </summary>
        public static int RangerInsertPation { get { return 25; } }

        /// <summary>
        /// Creates AutoStatements in the size of RangerInsertPation
        /// </summary>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        public void InsertRange<T>(IEnumerable<T> entry)
        {
            Database.RunInTransaction(c =>
            {
                //foreach (var item in entry)
                //{
                //    this.ExecuteGenericCommand(_CreateInsert(item, c));
                //}

                var insertRangeCommand = CreateInsertRangeCommand(entry.ToArray(), c);

                foreach (var item in insertRangeCommand)
                {
                    RaiseKnownInsert(item);
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
            var resultSet = new List<IDataParameter>();
            var stringBuilder = new StringBuilder();

            var commands = new List<IDbCommand>();

            int toke = 0;

            for (int i = 0; i < entrys.Count(); i++)
            {
                var singelCommand = _CreateInsert(typeof(T), entrys[i], db);
                var singelCommandText = singelCommand.CommandText;
                var singelParamter = singelCommand.Parameters.Cast<IDataParameter>();
                foreach (var parameter in singelParamter)
                {
                    var newName = parameter.ParameterName + "" + i;
                    singelCommandText = singelCommandText.Replace(parameter.ParameterName, newName);
                    resultSet.Add(db.CreateParameter(newName, parameter.Value));
                }

                stringBuilder.AppendLine(singelCommandText + ";");
                toke++;

                if (toke == RangerInsertPation)
                {
                    toke = 0;
                    //Create MultiCommand
                    var dbDataParameters = resultSet.ToArray();
                    commands.Add(db.CreateCommand(stringBuilder.ToString(), dbDataParameters));
                    stringBuilder.Clear();
                    resultSet.Clear();
                }
            }

            if (toke != 0)
            {
                commands.Add(db.CreateCommand(stringBuilder.ToString(), resultSet.ToArray()));
                stringBuilder.Clear();
            }

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
                type.GetProperties()
                    .Where(
                        s =>
                            s.CheckForPK() ||
                            s.GetCustomAttributes().Any(e => e is InsertIgnore || e is IgnoreReflectionAttribute))
                    .Select(s => s.Name)
                    .Concat(CreateIgnoreList(type))
                    .ToArray();
            string[] propertyInfos = CreatePropertyNames(type, ignore).ToArray();
            string csvprops = CreatePropertyCSV(type, ignore);

            string values = "";
            for (int index = 0; index < propertyInfos.Length; index++)
                values = values + ("@" + index + ",");
            values = values.Remove(values.Length - 1);
            string query = "INSERT INTO " + type.GetTableName() + " ( " + csvprops + " ) VALUES ( " + values + " )";

            string[] orignialProps = type.GetPropertysViaRefection(ignore).ToArray();

            return CreateCommandWithParameterValues(type, query, orignialProps, entry, db);
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
            return CheckInstanceForAttriute<InsertFactoryMethodAttribute>(type, entry, db, (e, f) => _CreateInsert(type, e, f), parameter);
        }

        /// <summary>
        /// Creates and Executes a Insert statement for a given <param name="entry"></param>
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        public static void Insert<T>(T entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateInsert(typeof(T), entry, s)); });
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
                var dbCommand = CreateInsert(type, entry, s);
                var mergeCommands = MergeCommands(s, dbCommand, s.GetlastInsertedIdCommand());
                RaiseUnknwonInsert(mergeCommands);
                var skalar = s.GetSkalar(mergeCommands);
                object getlastInsertedId = skalar;
                return Select(type, Convert.ToInt64(getlastInsertedId), s);
            });
        }

        /// <summary>
        /// Not Connection save
        /// Must be executed inside a Valid Connection
        /// </summary>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public static IDbCommand ConcatCommands(IDatabase db, IDbCommand first, IDbCommand last)
        {
            var mergedCommandText = first.CommandText + " " + last.CommandText;
            var paramter = new List<IQueryParameter>();

            foreach (IDataParameter parameter in first.Parameters.Cast<IDataParameter>())
            {
                if (paramter.Any(s => s.Name == parameter.ParameterName))
                {
                    throw new ArgumentOutOfRangeException("first", "");
                }

                paramter.Add(new QueryParameter() { Name = parameter.ParameterName, Value = parameter.Value });
            }

            foreach (var parameter in last.Parameters.Cast<IDataParameter>())
            {
                if (paramter.Any(s => s.Name == parameter.ParameterName))
                {
                    throw new ArgumentOutOfRangeException("first", "");
                }

                paramter.Add(new QueryParameter() { Name = parameter.ParameterName, Value = parameter.Value });
            }

            return CreateCommandWithParameterValues(mergedCommandText, db, paramter);
        }

        /// <summary>
        /// Not Connection save
        /// Must be executed inside a Valid Connection
        /// </summary>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public static IDbCommand MergeCommands(IDatabase db, IDbCommand first, IDbCommand last)
        {
            var mergedCommandText = first.CommandText + ";" + last.CommandText;
            var paramter = new List<IQueryParameter>();

            foreach (IDataParameter parameter in first.Parameters.Cast<IDataParameter>())
            {
                if (paramter.Any(s => s.Name == parameter.ParameterName))
                {
                    throw new ArgumentOutOfRangeException("first", "");
                }

                paramter.Add(new QueryParameter() { Name = parameter.ParameterName, Value = parameter.Value });
            }

            foreach (var parameter in last.Parameters.Cast<IDataParameter>())
            {
                if (paramter.Any(s => s.Name == parameter.ParameterName))
                {
                    throw new ArgumentOutOfRangeException("first", "");
                }

                paramter.Add(new QueryParameter() { Name = parameter.ParameterName, Value = parameter.Value });
            }

            return CreateCommandWithParameterValues(mergedCommandText, db, paramter);
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
            return (T)InsertWithSelect(typeof(T) ,entry, db);
        }
    }
}