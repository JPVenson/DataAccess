using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess
{
    public static class DbAccessLayerHelper
    {
        /// <summary>
        /// Wraps a <param name="query"></param> on a given <param name="type"></param> by including <param name="entry"></param>'s 
        /// propertys that are defined in <param name="propertyInfos"></param>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, Type type, string query, string[] propertyInfos, object entry)
        {
            object[] propertyvalues =
                propertyInfos.Select(
                    propertyInfo =>
                    {
                        PropertyInfo property =
                            type.GetProperty(type.ReMapSchemaToEntiysProp(propertyInfo));
                        object dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
                        return dataValue;
                    }).ToArray();
            return db.CreateCommandWithParameterValues(query, propertyvalues);
        }

        /// <summary>
        /// Wraps a <param name="query"></param> on a given typeof(T) by including <param name="entry"></param>'s 
        /// propertys that are defined in <param name="propertyInfos"></param>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="entry"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues<T>(this IDatabase db, string query, string[] propertyInfos, T entry)
        {
            return db.CreateCommandWithParameterValues(typeof(T), query, propertyInfos, entry);
        }

        /// <summary>
        /// Wraps <param name="query"></param> into a Command and adds the values
        /// values are added by Index
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query, object[] values)
        {
            var listofQueryParamter = new List<IQueryParameter>();
            for (int i = 0; i < values.Count(); i++)
                listofQueryParamter.Add(new QueryParameter { Name = i.ToString(CultureInfo.InvariantCulture), Value = values[i] });
            return db.CreateCommandWithParameterValues(query, listofQueryParamter);
        }

        /// <summary>
        /// Wraps <param name="query"></param> into a Command and adds the values
        /// values are added by Name of IQueryParamter
        /// If item of <param name="values"></param> contains a name that does not contains @ it will be added
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query, IEnumerable<IQueryParameter> values)
        {
            IDbCommand cmd = CreateCommand(db, query);
            if (values != null)
                foreach (IQueryParameter queryParameter in values)
                {
                    IDbDataParameter dbDataParameter = cmd.CreateParameter();
                    dbDataParameter.Value = queryParameter.Value;
                    dbDataParameter.ParameterName = !queryParameter.Name.StartsWith("@")
                        ? "@" + queryParameter.Name
                        : queryParameter.Name;
                    cmd.Parameters.Add(dbDataParameter);
                }
            return cmd;
        }

        internal static IEnumerable<IQueryParameter> EnumarateFromDynamics(dynamic parameter)
        {
            var list = new List<IQueryParameter>();

            PropertyInfo[] propertys = ((Type)parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                dynamic value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter { Name = "@" + element.Name, Value = value });
            }

            return list;
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignorePk"></param>
        /// <returns></returns>
        public static string CreatePropertyCSV(this Type type, bool ignorePk = false)
        {
            return CreatePropertyNames(type, ignorePk).Aggregate((e, f) => e + ", " + f);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection
        /// </summary>
        /// <param name="ignorePk"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string CreatePropertyCSV<T>(bool ignorePk = false)
        {
            return CreatePropertyCSV(typeof(T), ignorePk);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        internal static string CreatePropertyCSV(this Type type, params string[] ignore)
        {
            IEnumerable<string> propertyNames = CreatePropertyNames(type, ignore);
            return propertyNames.Aggregate((e, f) => e + ", " + f);
        }

        /// <summary>
        /// Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        internal static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyCSV(typeof(T), ignore);
        }

        /// <summary>
        /// Maps all propertys of <param name="type"></param> into the Database columns
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNames(this Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        /// <summary>
        /// Maps all propertys of typeof(T) into the Database columns
        /// </summary>
        /// <param name="ignore"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            return CreatePropertyNames(typeof(T), ignore);
        }

        internal static List<IDataRecord> EnumerateDataRecords(this IDatabase database, IDbCommand query)
        {
            return database.Run(
                s =>
                {
                    //Skip enumeration and make a Direkt loading
                    //This increeses Performance

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

        /// <summary>
        /// Maps propertys to database of given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignorePK"></param>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames(type, type.GetPK()) : CreatePropertyNames(type, new string[0]);
        }

        /// <summary>
        /// Maps propertys to database of given type
        /// </summary>
        /// <param name="ignorePK"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof(T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }

        /// <summary>
        /// Gets all propertys that should be ignored due rules
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] CreateIgnoreList(this Type type)
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

        /// <summary>
        /// Wraps a Parameterless string into a Command for the given DB
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(this IDatabase db, string query)
        {
            return db.CreateCommand(query);
        }


        /// <summary>
        /// Runs a Command on a given Database and Converts the Output into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="db"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ExecuteGenericCreateModelsCommand<T>(this IDbCommand command, IDatabase db)
            where T : class, new()
        {
            return db.Run(
                s =>
                    s.GetEntitiesList(command, DataConverterExtensions.SetPropertysViaReflection<T>)
                        .ToList());
        }

        /// <summary>
        /// Execute a Query on a given Database
        /// </summary>
        /// <param name="command"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static int ExecuteGenericCommand(this IDbCommand command, IDatabase db)
        {
            return db.Run(s => s.ExecuteNonQuery(command));
        }


        internal static IDbCommand CheckInstanceForAttriute<TE>(this Type type, object entry, IDatabase db,
Func<object, IDatabase, IDbCommand> fallback, params object[] param)
where TE : DataAccessAttribute
        {
            //try to get a Factory mehtod
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(DbType)*/));

            MethodInfo[] methods =
                type.GetMethods().Where(s => s.GetCustomAttributes(false).Any(e => e is TE)).ToArray();

            if (methods.Any())
            {
                MethodInfo[] searchMethodWithFittingParams = methods.Where(s =>
                {
                    ParameterInfo[] parameterInfos = s.GetParameters();

                    if (parameterInfos.Length != param.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo para = parameterInfos[i];
                        object tryParam = param[i];
                        if (tryParam == null)
                            return false;
                        if (!(para.ParameterType == para.GetType()))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToArray();

                if (searchMethodWithFittingParams.Length != 1)
                {
                    return fallback(entry, db);
                }

                MethodInfo method = searchMethodWithFittingParams.First();

                //must be public static
                if (!method.IsStatic)
                {
                    object[] cleanParams = param != null && param.Any() ? param : null;
                    object invoke = method.Invoke(entry, cleanParams);
                    if (invoke != null)
                    {
                        if (invoke is string && !String.IsNullOrEmpty(invoke as string))
                        {
                            return CreateCommand(db, invoke as string);
                        }
                        if (invoke is IQueryFactoryResult)
                        {
                            var result = invoke as IQueryFactoryResult;
                            return db.CreateCommandWithParameterValues(result.Query, result.Parameters);
                        }
                    }
                }
            }
            return fallback(entry, db);
        }

        internal static IDbCommand CheckInstanceForAttriute<T, TE>(this Type type, T entry, IDatabase db,
            Func<T, IDatabase, IDbCommand> fallback, params object[] param)
            where TE : DataAccessAttribute
        {
            return CheckInstanceForAttriute<TE>(type, entry, db, (o, database) => fallback((T)o, database), param);
        }

        internal static IDatabaseStrategy GenerateStrategy(this string fullValidIdentifyer, string connection)
        {
            if (String.IsNullOrEmpty(fullValidIdentifyer))
                throw new ArgumentException("Type was not found");

            Type type = Type.GetType(fullValidIdentifyer);
            if (type == null)
            {
                IEnumerable<string> parallelQuery = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll",
                    SearchOption.TopDirectoryOnly);

                Assembly assam = null;

                Parallel.ForEach(parallelQuery, (s, e) =>
                {
                    Assembly loadFile = Assembly.LoadFile(s);
                    Type resolve = loadFile.GetType(fullValidIdentifyer);
                    if (resolve != null)
                    {
                        type = resolve;
                        assam = loadFile;
                        e.Break();
                    }
                });

                if (type == null)
                    throw new ArgumentException("Type was not found");
            }

            //check the type to be a Strategy

            if (!type.GetInterfaces().Contains(typeof(IDatabaseStrategy)))
            {
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
            }

            //try constructor injection
            ConstructorInfo ctOfType =
                type.GetConstructors()
                    .FirstOrDefault(
                        s => s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof(string));
            if (ctOfType != null)
            {
                return ctOfType.Invoke(new object[] { connection }) as IDatabaseStrategy;
            }
            var instanceOfType = Activator.CreateInstance(type) as IDatabaseStrategy;
            if (instanceOfType == null)
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");

            instanceOfType.ConnectionString = connection;

            return instanceOfType;
        }
    }
}
