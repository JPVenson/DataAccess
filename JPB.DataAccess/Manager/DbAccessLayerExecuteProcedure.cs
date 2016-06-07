/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.Manager
{
	partial class DbAccessLayer
	{
		/// <summary>
		/// </summary>
		public static Dictionary<Type, DbType> DbTypeMap;

		/// <summary>
		/// Map an SourceDbType to the proper C# mscore lib type
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static Type Map(DbType source)
		{
			return DbTypeMap.FirstOrDefault(s => s.Value == source).Key;
		}
		/// <summary>
		/// Map an C# mscore lib type to the proper SourceDbType
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static DbType? Map(Type source)
		{
			DbType result;
			DbTypeMap.TryGetValue(source, out result);
			return result;
		}

		/// <summary>
		/// </summary>
		private static void SProcedureDbAccessLayer()
		{
			//not that cool maybe move this to another place or using the exisiting mehtods
			DbTypeMap = new Dictionary<Type, DbType>();

			DbTypeMap[typeof(byte)] = DbType.Byte;
			DbTypeMap[typeof(sbyte)] = DbType.SByte;
			DbTypeMap[typeof(short)] = DbType.Int16;
			DbTypeMap[typeof(ushort)] = DbType.UInt16;
			DbTypeMap[typeof(int)] = DbType.Int32;
			DbTypeMap[typeof(uint)] = DbType.UInt32;
			DbTypeMap[typeof(long)] = DbType.Int64;
			DbTypeMap[typeof(ulong)] = DbType.UInt64;
			DbTypeMap[typeof(float)] = DbType.Single;
			DbTypeMap[typeof(double)] = DbType.Double;
			DbTypeMap[typeof(decimal)] = DbType.Decimal;
			DbTypeMap[typeof(bool)] = DbType.Boolean;
			DbTypeMap[typeof(string)] = DbType.String;
			DbTypeMap[typeof(char)] = DbType.StringFixedLength;
			DbTypeMap[typeof(Guid)] = DbType.Guid;
			DbTypeMap[typeof(DateTime)] = DbType.DateTime;
			DbTypeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;

			DbTypeMap[typeof(byte[])] = DbType.Binary;
			DbTypeMap[typeof(byte?)] = DbType.Byte;
			DbTypeMap[typeof(sbyte?)] = DbType.SByte;
			DbTypeMap[typeof(short?)] = DbType.Int16;
			DbTypeMap[typeof(ushort?)] = DbType.UInt16;
			DbTypeMap[typeof(int?)] = DbType.Int32;
			DbTypeMap[typeof(uint?)] = DbType.UInt32;
			DbTypeMap[typeof(long?)] = DbType.Int64;
			DbTypeMap[typeof(ulong?)] = DbType.UInt64;
			DbTypeMap[typeof(float?)] = DbType.Single;
			DbTypeMap[typeof(double?)] = DbType.Double;
			DbTypeMap[typeof(decimal?)] = DbType.Decimal;
			DbTypeMap[typeof(bool?)] = DbType.Boolean;
			DbTypeMap[typeof(char?)] = DbType.StringFixedLength;
			DbTypeMap[typeof(Guid?)] = DbType.Guid;
			DbTypeMap[typeof(DateTime?)] = DbType.DateTime;
			DbTypeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
		}

		/// <summary>
		///     Executes a Procedure object into the Database
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void ExecuteProcedureNonResult<T>(T procParam)
		{
			ExecuteProcedureNonResult(typeof(T), procParam);
		}

		/// <summary>
		///     Executes a Procedure object into the Database
		/// </summary>
		public void ExecuteProcedureNonResult(Type type, object procParam)
		{
			var command = CreateProcedureCall(type.GetClassInfo(), procParam, Database);
			Database.ExecuteNonQuery(command);
		}

		/// <summary>
		///     Executes a Procedure object into the Database
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The result</typeparam>
		public TE[] ExecuteProcedure<T, TE>(T procParam)
		{
			return ExecuteProcedure(typeof(T), typeof(TE), procParam).Cast<TE>().ToArray();
		}

		/// <summary>
		///     Executes a Procedure object into the Database
		/// </summary>
		public IEnumerable ExecuteProcedure(Type procParamType, Type resultType, object procParam)
		{
			var command = CreateProcedureCall(procParamType.GetClassInfo(), procParam, Database);
			var typeInfo = resultType.GetClassInfo();
			return EnumerateDataRecords(command, LoadCompleteResultBeforeMapping, typeInfo);
			//if (LoadCompleteResultBeforeMapping)
			//{
			//	return EnumerateDataRecords(command)
			//		.Select(s => SetPropertysViaReflection(typeInfo, s))
			//		.ToList();
			//}
			//else
			//{
			//	return EnumerateDirectDataRecords(command, typeInfo);
			//}
		}

		/// <summary>
		///     Executes a Procedure object into the Database
		/// </summary>
		public object[] ExecuteProcedurePrimetiv(Type procParamType, Type resultType, object procParam)
		{
			var command = CreateProcedureCall(procParamType.GetClassInfo(), procParam, Database);
			return EnumerateDataRecords(command)
				.Select(dataRecord => dataRecord[0])
				.ToArray();
		}

		private static IDbCommand CreateProcedureCall(DbClassInfoCache procParamType, object procParam, IDatabase db)
		{
			var sb = new StringBuilder();
			sb.Append("EXECUTE ");
			sb.Append(procParamType.TableName);
			sb.Append(" ");
			var procParams = CreateProcedureHeader(procParamType).ToArray();
			var count = 0;
			foreach (var queryParameter in procParams)
			{
				count++;
				sb.Append(queryParameter.Name.CheckParamter());
				sb.Append(" ");
				if (count < procParams.Length)
					sb.Append(", ");
			}

			var procedureProcessor = new ProcedureProcessor(sb.ToString());
			procedureProcessor.QueryParameters = procParams;
			ValidateEntity(procParam);
			return procedureProcessor.CreateCommand(procParam, db);
		}

		private static IEnumerable<IQueryParameter> CreateProcedureHeader(DbClassInfoCache type)
		{
			return
				type.Propertys
					.Select(
						propertyInfo => new QueryParameter(propertyInfo.Value.DbName, propertyInfo.Value.PropertyType));
		}

		private interface IProcedureProcessor
		{
			string Query { get; }
			DbClassInfoCache TargetType { set; }
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

			public DbClassInfoCache TargetType { set; get; }

			public IDbCommand CreateCommand(object target, IDatabase db)
			{
				if (TargetType == null)
					TargetType = target.GetType().GetClassInfo();

				var dbCommand = db.CreateCommand(Query);

				foreach (var queryParameter in QueryParameters)
				{
					var realName = TargetType.GetDbToLocalSchemaMapping(queryParameter.Name);
					var value = TargetType.Propertys[realName].Getter.Invoke(target);
					dbCommand.Parameters.AddWithValue(queryParameter.Name.CheckParamter(), value, db);
				}

				return dbCommand;
			}
		}
	}
}

//private static IDbCommand CreateExecuteQueryFactory(Type procParamType, IDatabase db, params object[] parameter)
//{
//    //try to get the attribute for static selection
//    if (parameter != null && !parameter.Any())
//    {
//        var staticFactory = procParamType.GetCustomAttributes().FirstOrDefault(s => s is StoredProcedureFactoryAttribute) as StoredProcedureFactoryAttribute;

//        if (staticFactory != null)
//        {
//            return CreateProcedureCall(procParamType);
//            //return DbAccessLayerHelper.CreateCommand(db, staticFactory.QueryCommand);
//        }
//    }

//    //try to get a Factory mehtod
//    //var methods =
//    //    procParamType.GetMethods()
//    //        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(dbAccessType)*/));

//    MethodInfo[] methods =
//        procParamType.GetMethods()
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
//            return CreateProcedureCall(procParamType);
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
//                    return db.CreateCommandWithParameterValues(result.QueryCommand, result.Parameters);
//                }
//            }
//        }
//    }
//    return CreateProcedureCall(procParamType);
//}