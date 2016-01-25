using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
	public partial class DbAccessLayer
	{
		/// <summary>
		///     If enabled Related structures will be loaded into the source object
		/// </summary>
		public static bool ProcessNavigationPropertys { get; set; }

		#region BasicCommands

		/// <summary>
		///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk" />
		/// </summary>
		/// <returns></returns>
		public object Select(Type type, object pk)
		{
			return Select(type, pk, Database, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Select<T>(object pk)
		{
			return (T) Select(typeof (T), pk);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		protected static object Select(Type type, object pk, IDatabase db, bool egarLoading)
		{
			return Select(type, db, CreateSelect(type, db, pk), egarLoading).FirstOrDefault();
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected static T Select<T>(object pk, IDatabase db, bool egarLoading)
		{
			//return Select<T>(db, CreateSelect<T>(db, pk)).FirstOrDefault();
			return (T) Select(typeof (T), pk, db, egarLoading);
		}

		/// <summary>
		///     Creates and Executes a Plain select over a
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] Select(Type type, params object[] parameter)
		{
			return Select(type, Database, LoadCompleteResultBeforeMapping, parameter);
		}

		/// <summary>
		///     Uses a Factory method to Generate a new set of T
		///     When no Factory is found an Reflection based Factory is used
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] Select<T>(params object[] parameter)
		{
			var objects = Select(typeof (T), parameter);
			return objects.Cast<T>().ToArray();
		}

		/// <summary>
		///     Creates and Executes a SelectStatement for a given
		///     <paramref name="type" />
		///     by using the
		///     <paramref name="parameter" />
		/// </summary>
		/// <returns></returns>
		protected static object[] Select(Type type, IDatabase db, bool egarLoading, params object[] parameter)
		{
			return Select(type, db, CreateSelectQueryFactory(type.GetClassInfo(), db, parameter), egarLoading);
		}

		/// <summary>
		///     Creates a selectStatement for a given
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected static T[] Select<T>(IDatabase db, bool egarLoading)
		{
			return Select(typeof (T), db, egarLoading).Cast<T>().ToArray();
		}

		/// <summary>
		///     Creates and Executes a Select Statement for a given
		///     <paramref name="type" />
		///     by using
		///     <paramref name="command" />
		/// </summary>
		/// <returns></returns>
		protected static object[] Select(Type type, IDatabase db, IDbCommand command, bool egarLoading)
		{
			return SelectNative(type, db, command, egarLoading);
		}

		/// <summary>
		///     Creates and Executes a Select Statement for
		///     <typeparam name="T"></typeparam>
		///     by using
		///     <paramref name="command" />
		/// </summary>
		/// <returns></returns>
		protected static T[] Select<T>(IDatabase db, IDbCommand command, bool egarLoading)
		{
			return Select(typeof (T), db, command, egarLoading).Cast<T>().ToArray();
		}

		#endregion

		#region CreateCommands

		/// <summary>
		///     Creates a Select with appended query
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateSelect(Type type, IDatabase db, string query)
		{
			return DbAccessLayerHelper.CreateCommand(db,
				CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " " + query);
		}


		/// <summary>
		///     Creates a Select with appended query
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateSelect<T>(IDatabase db, string query)
		{
			return CreateSelect(typeof (T), db, query);
		}

		/// <summary>
		///     Creates a Select with appended query and inclueded Query Paramater
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateSelect(Type type, IDatabase db, string query, IEnumerable<IQueryParameter> paramenter)
		{
			var plainCommand = DbAccessLayerHelper.CreateCommand(db,
				CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " " + query);
			foreach (IQueryParameter para in paramenter)
				plainCommand.Parameters.AddWithValue(para.Name, para.Value, db);
			return plainCommand;
		}

		/// <summary>
		///     Creates a Select with appended query and inclueded Query Paramater
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IDbCommand CreateSelect<T>(IDatabase db, string query,
			IEnumerable<IQueryParameter> paramenter)
		{
			return CreateSelect(typeof (T), db, query, paramenter);
		}

		internal static IDbCommand CreateSelectQueryFactory(DbClassInfoCache type, IDatabase db, params object[] parameter)
		{
			//try to get the attribute for static selection
			if (parameter != null && !parameter.Any())
			{
				if (type.SelectFactory != null)
				{
					return DbAccessLayerHelper.CreateCommand(db, type.SelectFactory.Attribute.Query);
				}
			}

			//try to get a Factory mehtod
			//var methods =
			//    type.GetMethods()
			//        .FirstOrDefault(s => s.GetCustomAttributes(false).Any(e => e is TE /*&& (e as TE).DbQuery.HasFlag(dbAccessType)*/));

			var methods =
				type.MethodInfoCaches
					.Where(s => s.AttributeInfoCaches.Any(e => e.Attribute is SelectFactoryMethodAttribute))
					.ToArray();

			if (methods.Any())
			{
				var searchMethodWithFittingParams = methods.Where(s =>
				{
					var parameterInfos = s.MethodInfo.GetParameters();

					if (parameterInfos.Length != parameter.Length)
					{
						return false;
					}

					for (var i = 0; i < parameterInfos.Length; i++)
					{
						var para = parameterInfos[i];
						var tryParam = parameter[i];
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
					return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type.Type));
				}

				var method = searchMethodWithFittingParams.First();

				//must be public static
				if (method.MethodInfo.IsStatic)
				{
					var cleanParams = parameter != null && parameter.Any() ? parameter : null;
					var invoke = method.MethodInfo.Invoke(null, cleanParams);
					if (invoke != null)
					{
						if (invoke is string && !string.IsNullOrEmpty(invoke as string))
						{
							return DbAccessLayerHelper.CreateCommand(db, invoke as string);
						}
						if (invoke is IQueryFactoryResult)
						{
							var result = invoke as IQueryFactoryResult;
							return db.CreateCommandWithParameterValues(result.Query, result.Parameters);
						}
					}
				}
			}
			return DbAccessLayerHelper.CreateCommand(db, CreateSelect(type.Type));
		}

		/// <summary>
		///     Creates a Select for one Item with appended query and inclueded Query Paramater
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateSelect(Type type, IDatabase db, object pk)
		{
			var proppk = type.GetPK();
			var query = CreateSelectQueryFactory(type.GetClassInfo(), db).CommandText + " WHERE " + proppk + " = @pk";
			var cmd = DbAccessLayerHelper.CreateCommand(db, query);
			cmd.Parameters.AddWithValue("@pk", pk, db);
			return cmd;
		}

		/// <summary>
		///     Creates a Select for one Item with appended query and inclueded Query Paramater
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IDbCommand CreateSelect<T>(IDatabase db, object pk)
		{
			return CreateSelect(typeof (T), db, pk);
		}

		/// <summary>
		///     Creates a Plain Select statement by using
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static string CreateSelect(Type type)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			sb.Append(type.CreatePropertyCsv(
				type
					.GetClassInfo()
					.PropertyInfoCaches
					.Where(f => f.Value.ForginKeyAttribute != null)
					.Select(f => f.Key)
					.ToArray()));
			sb.Append(" FROM ");
			sb.Append(type.GetTableName());
			return sb.ToString();
			//return "SELECT " + type.CreatePropertyCSV(type.CreateIgnoreList()) + " FROM " + type.GetTableName();
		}

		/// <summary>
		///     Creates a Select by using a Factory mehtod or auto generated querys
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IDbCommand CreateSelect<T>(IDatabase db)
		{
			return CreateSelectQueryFactory(typeof (T).GetClassInfo(), db);
		}

		#endregion

		#region Runs

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static IEnumerable RunDynamicSelect(Type type, IDatabase database, IDbCommand query, bool egarLoading)
		{
			RaiseSelect(query, database);
			var typeInfo = type.GetClassInfo();

			if (egarLoading)
			{
				var results = database.EnumerateDataRecords(query, true);
				var recordToNameMapping = new Dictionary<int, DbPropertyInfoCache>();

				if (!results.Any())
					return new ArrayList();

				var anyReader = results.First();

				for (var i = 0; i < anyReader.FieldCount; i++)
				{
					DbPropertyInfoCache val = null;
					typeInfo.PropertyInfoCaches.TryGetValue(typeInfo.SchemaMappingDatabaseToLocal(anyReader.GetName(i)),
						out val);
					recordToNameMapping.Add(i, val);
				}

				return
					results
						.Select(record => typeInfo.SetPropertysViaReflection(record, recordToNameMapping))
						.ToArray();
			}
			return database.EnumerateDirectDataRecords(query, typeInfo);
		}

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static object[] RunSelect(Type type, IDatabase database, IDbCommand query, bool egarLoading)
		{
			return RunDynamicSelect(type, database, query, egarLoading).Cast<object>().ToArray();
		}

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T[] RunSelect<T>(IDatabase database, IDbCommand query, bool egarLoading)
		{
			return RunSelect(typeof (T), database, query, egarLoading).Cast<T>().ToArray();
		}

		/// <summary>
		///     Executes
		///     <paramref name="query" />
		///     and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static object[] RunSelect(Type type, IDatabase database, string query,
			IEnumerable<IQueryParameter> paramenter, bool egarLoading)
		{
			return
				database.Run(
					s =>
					{
						var command = DbAccessLayerHelper.CreateCommand(s, query);

						foreach (IQueryParameter item in paramenter)
							command.Parameters.AddWithValue(item.Name, item.Value, s);
						return RunSelect(type, database, command, egarLoading);
					}
					);
		}

		/// <summary>
		///     Executes
		///     <paramref name="query" />
		///     and Parse the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T[] RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter,
			bool egarLoading)
		{
			return RunSelect(typeof (T), database, query, paramenter, egarLoading).Cast<T>().ToArray();
		}

		private object[] RunSelect(Type type, IDbCommand command)
		{
			return RunSelect(type, Database, command, LoadCompleteResultBeforeMapping);
		}

		private T[] RunSelect<T>(IDbCommand command)
		{
			return RunSelect(typeof (T), Database, command, LoadCompleteResultBeforeMapping).Cast<T>().ToArray();
		}

		#endregion

		#region SelectWhereCommands

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectWhere(Type type, String @where)
		{
			var query = CreateSelect(type, Database, @where);
			return RunSelect(type, query);
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectWhere<T>(String @where)
		{
			return SelectWhere(typeof (T), @where).Cast<T>().ToArray();
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		///     uses
		///     <paramref name="paramenter" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectWhere(Type type, String @where, IEnumerable<IQueryParameter> paramenter)
		{
			var query = CreateSelect(type, Database, @where, paramenter);
			return RunSelect(type, query);
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		///     uses
		///     <paramref name="paramenter" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter)
		{
			return SelectWhere(typeof (T), where, paramenter).Cast<T>().ToArray();
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		///     uses
		///     <paramref name="paramenter" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectWhere(Type type, String @where, dynamic paramenter)
		{
			//Concret declaration is nessesary because we are working with dynmaics, so the compiler has ne space to guess the type wrong
			IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);
			return SelectWhere(type, where, enumarateFromDynamics);
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		///     uses<paramref name="paramenter" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectWhere<T>(String @where, dynamic paramenter)
		{
			List<object> selectWhere = SelectWhere(typeof (T), @where, paramenter);
			return selectWhere.Cast<T>().ToArray();
		}

		#endregion

		#region PrimetivSelects

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses the first line of output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] RunPrimetivSelect(Type type, IDbCommand command)
		{
			RaiseSelect(command, Database);
			return Database.EnumerateDataRecords(command, LoadCompleteResultBeforeMapping).Select(s => s[0]).ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses the first line of output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] RunPrimetivSelect(Type type, string query, IEnumerable<IQueryParameter> paramerter)
		{
			return RunPrimetivSelect(type, Database.CreateCommandWithParameterValues(query, paramerter));
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses the first line of output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] RunPrimetivSelect(Type type, string query)
		{
			return RunPrimetivSelect(type, query, new List<IQueryParameter>());
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses the first line of output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] RunPrimetivSelect<T>(string query, dynamic parameters)
		{
			IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(parameters);
			var runPrimetivSelect = RunPrimetivSelect(typeof (T), query, enumarateFromDynamics);
			return runPrimetivSelect.Cast<T>().ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses the first line of output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] RunPrimetivSelect<T>(string query, IEnumerable<IQueryParameter> parameters)
		{
			return RunPrimetivSelect(typeof (T), query, parameters).Cast<T>().ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses the first line of output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] RunPrimetivSelect<T>(string query)
		{
			return RunPrimetivSelect<T>(query, new List<IQueryParameter>());
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectNative<T>(string query) where T : class
		{
			return SelectNative(typeof (T), query).Cast<T>().ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, string query)
		{
			return Select(type, Database, DbAccessLayerHelper.CreateCommand(Database, query));
		}


		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static object[] SelectNative(Type type, IDatabase database, IDbCommand command, bool multiRow, bool egarLoading)
		{
			if (!multiRow)
				return SelectNative(type, database, command, egarLoading);

			//var guessingRelations = new Dictionary<PropertyInfo, IDbCommand>();

			//var propertyInfos = type.GetFKs();
			//var primaryKeyName = type.GetPK();

			//foreach (var propertyInfo in propertyInfos)
			//{
			//    guessingRelations.Append(propertyInfo, database.CreateCommand(string.Format("JOIN {0} ON {0} = {1}", propertyInfo.DeclaringType.GetTableName(), primaryKeyName)));
			//}

			/*
			 * Due the fact that you are not able to anylse the Query in a way to ensure its will not effect the query self we
			 * are loading the result an then loading based on that the items             
			 */

			return
				RunSelect(type, database, command, egarLoading).AsParallel().Select(s => s.LoadNavigationProps(database)).ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static object[] SelectNative(Type type, IDatabase database, IDbCommand command, bool egarLoading)
		{
			var objects = RunSelect(type, database, command, egarLoading);

			if (ProcessNavigationPropertys && type.GetClassInfo().HasRelations)
				foreach (object model in objects)
					model.LoadNavigationProps(database);

			return objects.ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, IDbCommand command)
		{
			return SelectNative(type, Database, command, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, string query, IEnumerable<IQueryParameter> paramenter)
		{
			var dbCommand = Database.CreateCommandWithParameterValues(query, paramenter);
			return SelectNative(type, dbCommand);
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <typeparamref name="T" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectNative<T>(string query, IEnumerable<IQueryParameter> paramenter)
		{
			return RunSelect<T>(Database, query, paramenter, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, string query, dynamic paramenter)
		{
			IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);
			return SelectNative(type, query, enumarateFromDynamics);
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, IDbCommand command, dynamic paramenter)
		{
			IEnumerable<IQueryParameter> enumarateFromDynamics = DbAccessLayerHelper.EnumarateFromDynamics(paramenter);

			foreach (IQueryParameter enumarateFromDynamic in enumarateFromDynamics)
			{
				command.Parameters.AddWithValue(enumarateFromDynamic.Name, enumarateFromDynamic.Value, Database);
			}

			return RunSelect(type, command);
		}

		/// <summary>
		///     Runs
		///     <paramref name="query" />
		///     and parses output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectNative<T>(string query, dynamic paramenter)
		{
			var objects = (object[]) SelectNative(typeof (T), query, paramenter);
			return objects.Cast<T>().ToArray();
		}

		#endregion

		#region experimental

		private void SelectDbAccessLayer()
		{
		}

		#endregion

		/// <summary>
		///     Executes a IDbCommand that will return multibe result sets that will be parsed to the marsTypes in order they are
		///     provided
		/// </summary>
		/// <returns></returns>
		public List<List<object>> ExecuteMARS(IDbCommand bulk, params Type[] marsTypes)
		{
			var mars = Database.EnumerateMarsDataRecords(bulk, LoadCompleteResultBeforeMapping);
			var concatedMarsToType = new List<Tuple<DbClassInfoCache, List<IDataRecord>>>();
			for (var index = 0; index < mars.Count; index++)
			{
				var dataRecord = mars[index];
				var expectedResult = marsTypes[index];
				concatedMarsToType.Add(new Tuple<DbClassInfoCache, List<IDataRecord>>(expectedResult.GetClassInfo(), dataRecord));
			}
			var list =
				concatedMarsToType.Select(s => s.Item2.Select(e => s.Item1.SetPropertysViaReflection(e)).AsParallel().ToList())
					.AsParallel()
					.ToList();
			return list;
		}
	}
}