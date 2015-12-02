using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.DbCollection;

namespace JPB.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbAccessLayerHelper
    {

        /// <summary>
        /// Not Connection save
        /// Must be executed inside a Valid Connection
        /// </summary>
        /// <param name="base"></param>
        /// <param name="last"></param>
        /// <param name="autoRename">allows an Automatik renaming of multible Commands</param>
        /// <returns></returns>
        public static IDbCommand MergeCommands(this IDatabase db, IDbCommand @base, IDbCommand last, bool autoRename = false)
        {
            return db.MergeTextToParameters(@base, last, autoRename);
        }

        internal static IDbCommand MergeTextToParameters(this IDatabase db,
            IDbCommand @base,
            IDbCommand last,
            bool autoRename = false)
        {
            var parameter = new List<IQueryParameter>();

            foreach (IDataParameter item in @base.Parameters.Cast<IDataParameter>())
            {
                parameter.Add(new QueryParameter() { Name = item.ParameterName, Value = item.Value });
            }
            var commandText = last.CommandText;

            foreach (var item in last.Parameters.Cast<IDataParameter>())
            {
                if (parameter.Any(s => s.Name == item.ParameterName))
                {
                    //Parameter is found twice in both commands so rename it
                    if (!autoRename)
                    {
                        throw new ArgumentOutOfRangeException("@base", string.Format("The parameter {0} exists twice. Allow Auto renaming or change one of the commands", item.ParameterName));
                    }
                    else
                    {
                        int counter = 1;
                        var parameterName = item.ParameterName;
                        var buffParam = parameterName;
                        while (parameter.Any(s => s.Name == buffParam))
                        {
                            buffParam = string.Format("{0}_{1}", parameterName, counter);
                            counter++;
                        }
                        commandText = commandText.Replace(item.ParameterName, buffParam);

                        item.ParameterName = buffParam;
                    }
                }

                parameter.Add(new QueryParameter() { Name = item.ParameterName, Value = item.Value });
            }



            return db.CreateCommandWithParameterValues(@base.CommandText + "; " + commandText, parameter);
        }

        /// <summary>
        /// Creates a DbCollection for the specifiy type
        /// To Limit the output create a new Type and then define the statement
        /// </summary>
        /// <param name="layer"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DbCollection<T> CreateDbCollection<T>(this DbAccessLayer layer)
            where T : class, 
            INotifyPropertyChanged
        {
            return new DbCollection<T>(layer.Select<T>());
        }

        public static IEnumerable<IQueryParameter> AsQueryParameter(this IDataParameterCollection source)
        {
            return
                (from IDataParameter parameter in source
                 select new QueryParameter(parameter.ParameterName, parameter.Value));
        }

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
                        var property =
                            type.GetProperty(type.GetDbToLocalSchemaMapping(propertyInfo));
                        object dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
                        return dataValue;
                    }).ToArray();
            return db.CreateCommandWithParameterValues(query, propertyvalues);
        }

        /// <summary>
        /// Wrappes a String into a Command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="db"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(this string commandText, IDatabase db, object param = null)
        {
            return db.CreateCommand(commandText, EnumarateFromDynamics(param).FromUserDefinedToSystemParameters(db));
        }

        public static IDataParameter[] FromUserDefinedToSystemParameters(this IEnumerable<IQueryParameter> parma, IDatabase db)
        {
            return parma.Select(s => db.CreateParameter(s.Name, s.Value)).ToArray();
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
            if (values == null)
                return cmd;
            foreach (var queryParameter in values)
            {
                IDbDataParameter dbDataParameter = cmd.CreateParameter();
                dbDataParameter.Value = queryParameter.Value;
                dbDataParameter.ParameterName = queryParameter.Name.CheckParamter();
                cmd.Parameters.Add(dbDataParameter);
            }
            return cmd;
        }

        internal static IEnumerable<IQueryParameter> EnumarateFromDynamics(this object parameter)
        {
            if (parameter == null)
                return new IQueryParameter[0];

            if (parameter is IQueryParameter)
            {
                return new[] { parameter as IQueryParameter };
            }

            if (parameter is IEnumerable<IQueryParameter>)
            {
                return parameter as IEnumerable<IQueryParameter>;
            }

            return (from element in ConfigHelper.GetPropertiesEx(((Type)parameter.GetType()))
                    let value = DataConverterExtensions.GetParamaterValue(parameter, element.Name)
                    select new QueryParameter { Name = element.Name.CheckParamter(), Value = value }).Cast<IQueryParameter>()
                .ToList();
        }

        //internal static IEnumerable<IQueryParameter> EnumarateFromDynamics(dynamic parameter)
        //{
        //    if (parameter is IQueryParameter)
        //    {
        //        return new[] { parameter as IQueryParameter };
        //    }

        //    if (parameter is IEnumerable<IQueryParameter>)
        //    {
        //        return parameter;
        //    }

        //    return (from element in ((Type)parameter.GetType()).GetProperties()
        //            let value = DataConverterExtensions.GetParamaterValue(parameter, element.Name)
        //            select new QueryParameter { Name = element.Name.CheckParamter(), Value = value }).Cast<IQueryParameter>()
        //        .ToList();
        //}

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
            return CreatePropertyNamesAndMap(type, ignore).Aggregate((e, f) => e + ", " + f);
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
        internal static IEnumerable<string> CreatePropertyNamesAndMap(this Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        /// <summary>
        /// Maps all propertys of typeof(T) into the Database columns
        /// </summary>
        /// <param name="ignore"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNamesAndMap<T>(params string[] ignore)
        {
            return CreatePropertyNamesAndMap(typeof(T), ignore);
        }

        internal static List<IDataRecord> EnumerateDataRecords(this IDatabase database, IDbCommand query, bool egarLoading)
        {
            return EnumerateMarsDataRecords(database, query, egarLoading).FirstOrDefault();
        }

        internal static List<List<IDataRecord>> EnumerateMarsDataRecords(this IDatabase database, IDbCommand query, bool egarLoading)
        {
            return database.Run(
                s =>
                {
                    //Skip enumeration and make a Direct loading
                    //This increeses Performance

                    var records = new List<List<IDataRecord>>();

                    using (var dr = query.ExecuteReader())
                    {
                        try
                        {
                            do
                            {
                                var resultSet = new List<IDataRecord>();
                                while (dr.Read())
                                {
                                    if (egarLoading)
                                    {
                                        resultSet.Add(dr.CreateEgarRecord());
                                    }
                                    else
                                    {
                                        resultSet.Add(dr);
                                    }
                                }
                                records.Add(resultSet);

                            } while (dr.NextResult());
                        }
                        finally
                        {
                            dr.Close();
                        }
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
            return ignorePK ? CreatePropertyNamesAndMap(type, type.GetPK()) : CreatePropertyNamesAndMap(type, new string[0]);
        }

        /// <summary>
        /// Maps propertys to database of given type
        /// </summary>
        /// <param name="ignorePK"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNamesAndMap<T>(typeof(T).GetPK()) : CreatePropertyNamesAndMap<T>(new string[0]);
        }

        /// <summary>
        /// Gets all propertys that should be ignored due rules
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] CreateIgnoreList(this Type type)
        {
            return
                ConfigHelper.GetPropertiesEx(type)
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


        internal static IDbCommand CreateCommandOfClassAttribute<TE>(
            this Type type,
            object entry, 
            IDatabase db,
            Func<object, IDatabase, IDbCommand> fallback, 
            params object[] param)
            where TE : DataAccessAttribute
        {
            //try to get a Factory method
            //var methods =
            //    type.GetMethods()
            //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(dbAccessType)*/));

            var methods =
                ConfigHelper.GetMethods(type).Where(s => s.GetCustomAttributes(false).Any(e => e is TE)).ToArray();

            if (methods.Any())
            {
                var searchMethodWithFittingParams = methods.Where(s =>
                {
                    var parameterInfos = s.GetParameters();

                    if (parameterInfos.Length != param.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        var para = parameterInfos[i];
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

                var method = searchMethodWithFittingParams.Single();

                //must be public static if attribute is Select
                if (typeof(TE) != typeof(SelectFactoryMethodAttribute) 
                    || (typeof(TE) == typeof(SelectFactoryMethodAttribute) && method.IsStatic))
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
            return CreateCommandOfClassAttribute<TE>(type, entry, db, (o, database) => fallback((T)o, database), param);
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

                //Assembly assam = null;

                Parallel.ForEach(parallelQuery, (s, e) =>
                {
                    var loadFile = System.Reflection.Assembly.LoadFile(s);
                    Type resolve = loadFile.GetType(fullValidIdentifyer);
                    if (resolve != null)
                    {
                        type = resolve;
                        //assam = loadFile;
                        e.Break();
                    }
                });

                if (type == null)
                    throw new ArgumentException("Type was not found");
            }

            //check the type to be a Strategy

            if (!typeof(IDatabaseStrategy).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
            }

            //try constructor injection
            var ctOfType =
                type.GetConstructors()
                    .FirstOrDefault(s => s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof(string));
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
