using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        #region BasicCommands

        /// <summary>
        ///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk"></paramref>
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
            return (T) Select(typeof (T), pk);
        }

        protected static object Select(Type type, long pk, IDatabase db)
        {
            return Select(type, db).FirstOrDefault();
        }

        protected static T Select<T>(long pk, IDatabase db)
        {
            return Select<T>(db, CreateSelect<T>(db, pk)).FirstOrDefault();
        }

        public List<object> Select(Type type, params object[] parameter)
        {
            return Select(type, Database, parameter);
        }

        /// <summary>
        ///     Uses a Factory method to Generate a new set of T
        ///     When no Factory is found an Reflection based Factory is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public List<T> Select<T>(params object[] parameter)
        {
            List<object> objects = Select(typeof (T), parameter);
            return objects.Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase db, params object[] parameter)
        {
            return Select(type, db, CreateSelectQueryFactory(type, db, parameter));
        }

        protected static List<T> Select<T>(IDatabase db)
        {
            return Select(typeof (T), db).Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase db, IDbCommand command)
        {
            return SelectNative(type, db, command);
        }

        protected static List<T> Select<T>(IDatabase db, IDbCommand command)
        {
            return Select(typeof (T), db, command).Cast<T>().ToList();
        }

        #endregion

        #region CreateCommands

        public static IDbCommand CreateSelect(Type type, IDatabase db, string query)
        {
            return CreateCommand(db, CreateSelectQueryFactory(type, db).CommandText + " " + query);
        }

        public static IDbCommand CreateSelect<T>(IDatabase db, string query)
        {
            return CreateSelect(typeof (T), db, query);
        }

        public static IDbCommand CreateSelect(Type type, IDatabase db, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand plainCommand = CreateCommand(db,
                CreateSelectQueryFactory(type, db).CommandText + " " + query);
            foreach (IQueryParameter para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value, db);
            return plainCommand;
        }

        public static IDbCommand CreateSelect<T>(IDatabase db, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return CreateSelect(typeof (T), db, query, paramenter);
        }

        public static string[] CreateIgnoreList(Type type)
        {
            return
                type.GetProperties()
                    .Where(
                        s =>
                            s.GetGetMethod(false).IsVirtual ||
                            s.GetCustomAttributes().Any(e => e is IgnoreReflectionAttribute))
                    .Select(s => s.Name)
                    .ToArray();
        }

        private static IDbCommand CreateSelectQueryFactory(Type type, IDatabase db, params object[] parameter)
        {
            //try to get the attribute for static selection
            var staticFactory =
                type.GetCustomAttributes().FirstOrDefault(s => s is SelectFactoryAttribute) as SelectFactoryAttribute;

            if (staticFactory != null)
            {
                return CreateCommand(db, staticFactory.Query);
            }

            //try to get a Factory mehtod
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(DbType)*/));

            MethodInfo[] methods =
                type.GetMethods()
                    .Where(s => !s.IsConstructor && !s.IsSpecialName)
                    .Where(s => s.GetCustomAttributes(false).Any(e => e is SelectFactoryMehtodAttribute))
                    .ToArray();

            if (methods.Any())
            {
                MethodInfo[] searchMethodWithFittingParams = methods.Where(s =>
                {
                    ParameterInfo[] parameterInfos = s.GetParameters();

                    if (parameterInfos.Length != parameter.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo para = parameterInfos[i];
                        object tryParam = parameter[i];
                        if (tryParam == null)
                            return false;
                        if (!(para.ParameterType == tryParam.GetType()))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToArray();

                if (searchMethodWithFittingParams.Length != 1)
                {
                    return CreateCommand(db, CreateSelect(type));
                }

                MethodInfo method = searchMethodWithFittingParams.First();

                //must be public static
                if (method.IsStatic)
                {
                    object[] cleanParams = parameter != null && parameter.Any() ? parameter : null;
                    object invoke = method.Invoke(null, cleanParams);
                    if (invoke != null)
                    {
                        if (invoke is string && !string.IsNullOrEmpty(invoke as string))
                        {
                            return CreateCommand(db, invoke as string);
                        }
                        if (invoke is IQueryFactoryResult)
                        {
                            var result = invoke as IQueryFactoryResult;
                            return CreateCommandWithParameterValues(result.Query, db, result.Parameters);
                        }
                    }
                }
            }
            return CreateCommand(db, CreateSelect(type));
        }

        public static IDbCommand CreateSelect(Type type, IDatabase db, long pk)
        {
            string proppk = type.GetPK();
            string query = CreateSelect(type) + " WHERE " + proppk + " = @pk";
            IDbCommand cmd = CreateCommand(db, query);
            cmd.Parameters.AddWithValue("@pk", pk, db);
            return cmd;
        }

        public static IDbCommand CreateSelect<T>(IDatabase db, long pk)
        {
            return CreateSelect(typeof (T), db, pk);
        }

        public static string CreateSelect<T>()
        {
            return CreateSelect(typeof (T));
        }

        public static string CreateSelect(Type type)
        {
            return "SELECT " + CreatePropertyCSV(type, CreateIgnoreList(type)) + " FROM " + type.GetTableName();
        }

        public static IDbCommand CreateSelect<T>(IDatabase db)
        {
            return CreateSelectQueryFactory(typeof (T), db);
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

                    using (IDataReader dr = query.ExecuteReader())
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
            return
                EnumerateDataRecords(database, query)
                    .Select(dataRecord => DataConverterExtensions.SetPropertysViaRefection(type, dataRecord))
                    .ToList();
        }

        public static List<object> RunSelect(Type type, IDatabase database, IDbCommand query)
        {
            return RunDynamicSelect(type, database, query) as List<object>;
        }

        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query)
        {
            return RunSelect(typeof (T), database, query).Cast<T>().ToList();
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
            return RunSelect(typeof (T), database, query, paramenter).Cast<T>().ToList();
        }

        private List<object> RunSelect(Type type, IDbCommand command)
        {
            return RunSelect(type, Database, command);
        }

        private List<T> RunSelect<T>(IDbCommand command)
        {
            return RunSelect(typeof (T), Database, command).Cast<T>().ToList();
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
            return SelectWhere(typeof (T), @where).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand query = CreateSelect(type, Database, @where, paramenter);
            return RunSelect(type, query);
        }

        public List<T> SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter)
        {
            return SelectWhere(typeof (T), where, paramenter).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectWhere(type, where, enumarateFromDynamics);
        }

        public List<T> SelectWhere<T>(String @where, dynamic paramenter)
        {
            List<object> selectWhere = SelectWhere(typeof (T), @where, paramenter);
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
            return RunPrimetivSelect(typeof (T), query).Cast<T>().ToList();
        }

        public List<object> SelectNative(Type type, string query)
        {
            return Select(type, Database, CreateCommand(Database, query));
        }

        public List<T> SelectNative<T>(string query) where T : class
        {
            return SelectNative(typeof (T), query).Cast<T>().ToList();
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
            var objects = ((List<object>) SelectNative(typeof (T), query, paramenter));
            return objects.Cast<T>().ToList();
        }

        #endregion

        #region experimental

        private TestQueryProvider _testQueryProvider;

        private void SelectDbAccessLayer()
        {
            _testQueryProvider = new TestQueryProvider(this);
        }

        [Obsolete]
        public IQueryable<T> SelectQuery<T>()
        {
            MethodInfo makeGenericMethod = ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T));
            MethodCallExpression methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
            return _testQueryProvider.CreateQuery<T>(methodCallExpression);
        }

        #endregion
    }
}