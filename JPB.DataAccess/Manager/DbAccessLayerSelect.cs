using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.QueryProvider;
using JPB.DataAccess;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        #region BasicCommands
        /// <summary>
        /// Execute select on a database with a standard Where [Primary Key] = <paramref name="pk"></paramref>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public object Select(Type type, long pk)
        {
            return Select(type, pk, Database);
        }

        public T Select<T>(long pk)
        {
            return (T)Select(typeof(T), pk);
        }

        protected static object Select(Type type, long pk, IDatabase batchRemotingDb)
        {
            return Select(type, batchRemotingDb).FirstOrDefault();
        }

        protected static T Select<T>(long pk, IDatabase batchRemotingDb)
        {
            return Select<T>(batchRemotingDb, CreateSelect<T>(batchRemotingDb, pk)).FirstOrDefault();
        }

        public List<object> Select(Type type)
        {
            return Select(type, Database);
        }

        public List<T> Select<T>()
        {
            var objects = Select(typeof(T));
            return objects.Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase batchRemotingDb)
        {
            return Select(type, batchRemotingDb, CreateSelectQueryFactory(type, batchRemotingDb));
        }

        protected static List<T> Select<T>(IDatabase batchRemotingDb)
        {
            return Select(typeof(T), batchRemotingDb).Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase batchRemotingDb, IDbCommand command)
        {
            return SelectNative(type, batchRemotingDb, command);
        }

        protected static List<T> Select<T>(IDatabase batchRemotingDb, IDbCommand command)
        {
            return Select(typeof(T), batchRemotingDb, command).Cast<T>().ToList();
        }

        #endregion

        #region CreateCommands

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, string query)
        {
            return CreateCommand(batchRemotingDb, CreateSelectQueryFactory(type, batchRemotingDb).CommandText + " " + query);
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query)
        {
            return CreateSelect(typeof(T), batchRemotingDb, query);
        }

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand plainCommand = CreateCommand(batchRemotingDb,
                CreateSelectQueryFactory(type, batchRemotingDb).CommandText + " " + query);
            foreach (IQueryParameter para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value, batchRemotingDb);
            return plainCommand;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return CreateSelect(typeof(T), batchRemotingDb, query, paramenter);
        }

        public static string[] CreateIgnoreList(Type type)
        {
            return
                type.GetProperties()
                    .Where(
                        s => s.GetGetMethod(false).IsVirtual)
                    .Select(s => s.Name)
                    .ToArray();
        }

        private static IDbCommand CreateSelectQueryFactory(Type type, IDatabase batchRemotingDb)
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

            //try to get the attribute for static selection
            var staticFactory = type.GetCustomAttributes().FirstOrDefault(s => s is SelectFactoryAttribute) as SelectFactoryAttribute;

            if (staticFactory != null)
            {
                return CreateCommand(batchRemotingDb, staticFactory.Query);
            }

            //try to get a Factory mehtod
            var methods =
                type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static)
                    .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is SelectFactoryMehtodAttribute));
            if (methods != null)
            {
                //must be public static
                if (methods.IsStatic)
                {
                    var invoke = methods.Invoke(null, null);
                    if (invoke is string)
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

            //screw that. Generate a select self!
            return CreateCommand(batchRemotingDb, CreateSelect(type));
        }

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, long pk)
        {
            string proppk = type.GetPK();
            string query = CreateSelect(type) + " WHERE " + proppk + " = @pk";
            IDbCommand cmd = CreateCommand(batchRemotingDb, query);
            cmd.Parameters.AddWithValue("@pk", pk, batchRemotingDb);
            return cmd;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, long pk)
        {
            return CreateSelect(typeof(T), batchRemotingDb, pk);
        }

        public static string CreateSelect<T>()
        {
            return CreateSelect(typeof(T));
        }

        public static string CreateSelect(Type type)
        {
            return "SELECT " + CreatePropertyCSV(type, CreateIgnoreList(type)) + " FROM " + type.GetTableName();
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb)
        {
            return CreateSelectQueryFactory(typeof(T), batchRemotingDb);
        }

        #endregion

        #region Runs

        //public static object RunDynamicSelect(Type type, IDatabase database, IDbCommand query)
        //{
        //    return
        //        database.Run(
        //            s => {}
        //                s.GetEntitiesList(query, e => DataConverterExtensions.SetPropertysViaRefection(type,e)).ToList());
        //}

        private static IEnumerable<IDataRecord> EnumerateDataRecords(IDatabase database, IDbCommand query)
        {
            return database.Run(
                s =>
                {
                    var records = new List<IDataRecord>();

                    using (var dr = query.ExecuteReader())
                    {
                        while (dr.Read())
                            records.Add(dr.CreateEgarRecord());
                        dr.Close();
                    }
                    return records;
                });
        }

        public static IEnumerable RunDynamicSelect(Type type, IDatabase database, IDbCommand query)
        {
            return EnumerateDataRecords(database, query).Select(dataRecord => DataConverterExtensions.SetPropertysViaRefection(type, dataRecord)).ToList();
        }

        public static List<object> RunSelect(Type type, IDatabase database, IDbCommand query)
        {
            return RunDynamicSelect(type, database, query) as List<object>;
        }

        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query)
        {
            return RunSelect(typeof(T), database, query).Cast<T>().ToList();
        }

        public static List<object> RunSelect(Type type, IDatabase database, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return
                database.Run(
                    s =>
                    {
                        IDbCommand command = CreateCommand(s, query);

                        foreach (IQueryParameter item in paramenter)
                            command.Parameters.AddWithValue(item.Name, item.Value, s);
                        return RunSelect(type, database, command);
                    }
                    );
        }

        public static List<T> RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter)
        {
            return RunSelect(typeof(T), database, query, paramenter).Cast<T>().ToList();
        }

        private List<object> RunSelect(Type type, IDbCommand command)
        {
            return RunSelect(type, Database, command);
        }

        private List<T> RunSelect<T>(IDbCommand command)
        {
            return RunSelect(typeof(T), Database, command).Cast<T>().ToList();
        }

        #endregion

        #region SelectWhereCommands

        public List<object> SelectWhere(Type type, String @where)
        {
            IDbCommand query = CreateSelect(type, Database, @where);
            return RunSelect(type, query);
        }

        public List<T> SelectWhere<T>(String @where)
        {
            return SelectWhere(typeof(T), @where).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand query = CreateSelect(type, Database, @where, paramenter);
            return RunSelect(type, query);
        }

        public List<T> SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter)
        {
            return SelectWhere(typeof(T), where, paramenter).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectWhere(type, where, enumarateFromDynamics);
        }

        public List<T> SelectWhere<T>(String @where, dynamic paramenter)
        {
            List<object> selectWhere = SelectWhere(typeof(T), @where, paramenter);
            return selectWhere.Cast<T>().ToList();
        }

        #endregion

        #region PrimetivSelects

        //public IEnumerable<object> RunPrimetivSelect(Type type, string query)
        //{
        //    return
        //        Database.Run(
        //            s =>
        //                s.GetEntitiesList(CreateCommand(s, query), e => e[0]).ToList());

        //}
        public IEnumerable<object> RunPrimetivSelect(Type type, string query)
        {
            return EnumerateDataRecords(Database, CreateCommand(Database, query)).Select(s => s[0]).ToList();
        }

        public List<T> RunPrimetivSelect<T>(string query) where T : class
        {
            return RunPrimetivSelect(typeof(T), query).Cast<T>().ToList();
        }

        public List<object> SelectNative(Type type, string query)
        {
            return Select(type, Database, CreateCommand(Database, query));
        }

        public List<T> SelectNative<T>(string query) where T : class
        {
            return SelectNative(typeof(T), query).Cast<T>().ToList();
        }

        public static List<object> SelectNative(Type type, IDatabase database, IDbCommand command)
        {
            List<object> objects = RunSelect(type, database, command);

            foreach (object model in objects)
                model.LoadNavigationProps(database);

            return objects;
        }

        public List<object> SelectNative(Type type, IDbCommand command)
        {
            return SelectNative(type, Database, command);
        }

        public List<object> SelectNative(Type type, string query, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand dbCommand = CreateCommandWithParameterValues(query, Database, paramenter);
            return SelectNative(type, dbCommand);
        }

        public List<T> SelectNative<T>(string query, IEnumerable<IQueryParameter> paramenter)
        {
            return RunSelect<T>(Database, query, paramenter);
        }

        public List<object> SelectNative(Type type, string query, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectNative(type, query, enumarateFromDynamics);
        }

        public List<T> SelectNative<T>(string query, dynamic paramenter)
        {
            var objects = ((List<object>)SelectNative(typeof(T), query, paramenter));
            return objects.Cast<T>().ToList();
        }

        #endregion

        #region experimental

        private TestQueryProvider _testQueryProvider;

        private void SelectDbAccessLayer()
        {
            _testQueryProvider = new TestQueryProvider(this);
        }

        //public IQueryable<T> SelectQuery<T>()
        //{
        //    var makeGenericMethod = ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T));
        //    var methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
        //    return _testQueryProvider.CreateQuery<T>(methodCallExpression);
        //}

        #endregion
    }
}