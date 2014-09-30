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
        public void Insert<T>(T entry)
        {
            Insert(entry, Database);
        }

        public T InsertWithSelect<T>(T entry)
        {
            return InsertWithSelect(entry, Database);
        }

        public static int RangerInsertPation { get { return 25; } }

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

        public static IDbCommand[] CreateInsertRangeCommand<T>(T[] entrys, IDatabase db)
        {
            var resultSet = new List<IDataParameter>();
            var stringBuilder = new StringBuilder();

            var commands = new List<IDbCommand>();

            int toke = 0;

            for (int i = 0; i < entrys.Count(); i++)
            {
                var singelCommand = _CreateInsert(entrys[i], db);
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

        public static IDbCommand _CreateInsert<T>(T entry, IDatabase db)
        {
            return _CreateInsert(typeof(T), entry, db);
        }

        public static IDbCommand CreateInsert(Type type, object entry, IDatabase db, params object[] parameter)
        {
            return CheckInstanceForAttriute<InsertFactoryMethodAttribute>(type, entry, db, (o, database) => _CreateInsert(type, o, database), parameter);
        }

        public static IDbCommand CreateInsert<T>(T entry, IDatabase db, params object[] parameter)
        {
            return CreateInsert(typeof(T), entry, db, parameter);
        }

        public static void Insert<T>(T entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateInsert(entry, s)); });
        }

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
                
                paramter.Add(new QueryParameter(){Name = parameter.ParameterName,Value = parameter.Value});
            }

            foreach (var parameter in last.Parameters.Cast<IDataParameter>())
            {
                if (paramter.Any(s => s.Name == parameter.ParameterName))
                {
                    throw new ArgumentOutOfRangeException("first", "");
                }

                paramter.Add(new QueryParameter(){Name = parameter.ParameterName,Value = parameter.Value});
            }

            return CreateCommandWithParameterValues(mergedCommandText, db, paramter);
        }

        public static T InsertWithSelect<T>(T entry, IDatabase db)
        {
            return (T)InsertWithSelect(typeof(T), entry, db);
        }
    }
}