using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        /// <summary>
        /// Executes a Procedure object into the Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ExecuteProcedureNonResult<T>(T target)
        {
            ExecuteProcedureNonResult(typeof(T), target);
        }

        /// <summary>
        /// Executes a Procedure object into the Database
        /// </summary>
        public void ExecuteProcedureNonResult(Type type, object target)
        {
            var command = CreateProcedureCall(type, target, Database);
            Database.ExecuteNonQuery(command);
        }

        /// <summary>
        /// Executes a Procedure object into the Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE">The result</typeparam>
        public List<TE> ExecuteProcedure<T, TE>(T target) where TE : class
        {
            return ExecuteProcedure(typeof(T), typeof(TE), target) as List<TE>;
        }

        /// <summary>
        /// Executes a Procedure object into the Database
        /// </summary>
        public List<object> ExecuteProcedure(Type type, Type resultType, object target)
        {
            var command = CreateProcedureCall(type, target, Database);
            return Database.EnumerateDataRecords(command, LoadCompleteResultBeforeMapping)
                .Select(dataRecord => DataConverterExtensions.SetPropertysViaReflection(resultType, dataRecord))
                .ToList();
        }

        //private static IDbCommand CreateExecuteQueryFactory(Type type, IDatabase db, params object[] parameter)
        //{
        //    //try to get the attribute for static selection
        //    if (parameter != null && !parameter.Any())
        //    {
        //        var staticFactory = type.GetCustomAttributes().FirstOrDefault(s => s is StoredProcedureFactoryAttribute) as StoredProcedureFactoryAttribute;

        //        if (staticFactory != null)
        //        {
        //            return CreateProcedureCall(type);
        //            //return DbAccessLayerHelper.CreateCommand(db, staticFactory.Query);
        //        }
        //    }

        //    //try to get a Factory mehtod
        //    //var methods =
        //    //    type.GetMethods()
        //    //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(DbType)*/));

        //    MethodInfo[] methods =
        //        type.GetMethods()
        //            .Where(s => !s.IsConstructor && !s.IsSpecialName)
        //            .Where(s => s.GetCustomAttributes(false).Any(e => e is SelectFactoryMehtodAttribute))
        //            .ToArray();

        //    if (methods.Any())
        //    {
        //        MethodInfo[] searchMethodWithFittingParams = methods.Where(s =>
        //        {
        //            ParameterInfo[] parameterInfos = s.GetParameters();

        //            if (parameterInfos.Length != parameter.Length)
        //            {
        //                return false;
        //            }

        //            for (int i = 0; i < parameterInfos.Length; i++)
        //            {
        //                ParameterInfo para = parameterInfos[i];
        //                object tryParam = parameter[i];
        //                if (tryParam == null)
        //                    return false;
        //                if (!(para.ParameterType == tryParam.GetType()))
        //                {
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }).ToArray();

        //        if (searchMethodWithFittingParams.Length != 1)
        //        {
        //            return CreateProcedureCall(type);
        //        }

        //        MethodInfo method = searchMethodWithFittingParams.First();

        //        //must be public static
        //        if (method.IsStatic)
        //        {
        //            object[] cleanParams = parameter != null && parameter.Any() ? parameter : null;
        //            object invoke = method.Invoke(null, cleanParams);
        //            if (invoke != null)
        //            {
        //                if (invoke is string && !string.IsNullOrEmpty(invoke as string))
        //                {
        //                    return DbAccessLayerHelper.CreateCommand(db, invoke as string);
        //                }
        //                if (invoke is IQueryFactoryResult)
        //                {
        //                    var result = invoke as IQueryFactoryResult;
        //                    return db.CreateCommandWithParameterValues(result.Query, result.Parameters);
        //                }
        //            }
        //        }
        //    }
        //    return CreateProcedureCall(type);
        //}

        private static IDbCommand CreateProcedureCall(Type t, object target, IDatabase db)
        {
            var sb = new StringBuilder();
            sb.Append("EXECUTE ");
            sb.Append(t.GetTableName());
            foreach (var queryParameter in CreateProcedureHeader(t))
            {
                sb.Append(queryParameter.Name);
            }
            var caller = new ProcedureProcessor(sb.ToString());
            return caller.CreateCommand(target, db);
        }

        private static IEnumerable<IQueryParameter> CreateProcedureHeader(Type t)
        {
            return t.GetProperties().Select(propertyInfo => new QueryParameter(t.MapEntiysPropToSchema(propertyInfo.Name), null));
        }

        interface IProcedureProcessor
        {
            string Query { get; }
            Type TargetType { set; }
            IEnumerable<IQueryParameter> QueryParameters { get; set; }
            IDbCommand CreateCommand(object target, IDatabase db);
        }

        private class ProcedureProcessor : IProcedureProcessor
        {
            public ProcedureProcessor(string query)
            {
                Query = query;
                QueryParameters = new List<IQueryParameter>();
            }

            public string Query { get; private set; }
            public IEnumerable<IQueryParameter> QueryParameters { get; set; }

            public Type TargetType { set; get; }

            public IDbCommand CreateCommand(object target, IDatabase db)
            {
                var dbCommand = db.CreateCommand(Query);

                foreach (var queryParameter in QueryParameters)
                {
                    var realName = TargetType.ReMapSchemaToEntiysProp(queryParameter.Name);
                    var value = TargetType.GetProperty(realName).GetValue(target);
                    dbCommand.Parameters.AddWithValue(queryParameter.Name, value, db);
                }

                return dbCommand;
            }
        }
    }
}
