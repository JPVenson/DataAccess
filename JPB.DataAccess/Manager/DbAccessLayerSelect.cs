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
using System.Reflection;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
	public partial class DbAccessLayer
	{
		/// <summary>
		///     If enabled Related structures will be loaded into the source object
		/// </summary>
		public bool ProcessNavigationPropertys { get; set; }

		#region BasicCommands

		/// <summary>
		///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk" />
		/// </summary>
		/// <returns></returns>
		public object Select(Type type, object pk)
		{
			return Select(type, pk, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] Select<T>()
		{
			return Select(typeof(T)).Cast<T>().ToArray();
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
			return (T)Select(typeof(T), pk);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		protected object Select(Type type, object pk, bool egarLoading)
		{
			return Select(type, CreateSelect(type, pk), egarLoading).FirstOrDefault();
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T Select<T>(object pk, bool egarLoading)
		{
			//return Select<T>(db, CreateSelect<T>(db, pk)).FirstOrDefault();
			return (T)Select(typeof(T), pk, egarLoading);
		}

		/// <summary>
		///     Creates and Executes a Plain select over a
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] Select(Type type, params object[] parameter)
		{
			return Select(type, LoadCompleteResultBeforeMapping, parameter);
		}

		/// <summary>
		///     Uses a Factory method to Generate a new set of T
		///     When no Factory is found an Reflection based Factory is used
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] Select<T>(object[] parameter)
		{
			var objects = Select(typeof(T), parameter);
			return objects.Cast<T>().ToArray();
		}

		/// <summary>
		///     Creates and Executes a SelectStatement for a given
		///     <paramref name="type" />
		///     by using the
		///     <paramref name="parameter" />
		/// </summary>
		/// <returns></returns>
		protected object[] Select(Type type, bool egarLoading, params object[] parameter)
		{
			return Select(type, CreateSelectQueryFactory(this.GetClassInfo(type), parameter), egarLoading);
		}

		/// <summary>
		///     Creates a selectStatement for a given
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T[] Select<T>(bool egarLoading)
		{
			return Select(typeof(T), egarLoading).Cast<T>().ToArray();
		}

		/// <summary>
		///     Creates and Executes a Select Statement for a given
		///     <paramref name="type" />
		///     by using
		///     <paramref name="command" />
		/// </summary>
		/// <returns></returns>
		protected object[] Select(Type type, IDbCommand command, bool egarLoading)
		{
			return SelectNative(type, command, egarLoading);
		}

		/// <summary>
		///     Creates and Executes a Select Statement for
		///     <typeparam name="T"></typeparam>
		///     by using
		///     <paramref name="command" />
		/// </summary>
		/// <returns></returns>
		protected T[] Select<T>(IDbCommand command, bool egarLoading)
		{
			return Select(typeof(T), command, egarLoading).Cast<T>().ToArray();
		}

		#endregion

		#region CreateCommands

		/// <summary>
		///     Creates a Select with appended query
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateSelect(Type type, string query)
		{
			return DbAccessLayerHelper.CreateCommand(Database,
				CreateSelectQueryFactory(this.GetClassInfo(type)).CommandText + " " + query);
		}


		/// <summary>
		///     Creates a Select with appended query
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateSelect<T>(string query)
		{
			return CreateSelect(typeof(T), query);
		}

		/// <summary>
		///     Creates a Select with appended query and inclueded QueryCommand Paramater
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateSelect(Type type, string query, IEnumerable<IQueryParameter> paramenter)
		{
			var plainCommand = Database.CreateCommand(
				CreateSelectQueryFactory(this.GetClassInfo(type)).CommandText + " " + query);
			if (paramenter != null)
				foreach (IQueryParameter para in paramenter)
					plainCommand.Parameters.AddWithValue(para.Name, para.Value, Database);
			return plainCommand;
		}

		/// <summary>
		///     Creates a Select with appended query and inclueded QueryCommand Paramater
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDbCommand CreateSelect<T>(string query,
			IEnumerable<IQueryParameter> paramenter)
		{
			return CreateSelect(typeof(T), query, paramenter);
		}

		/// <summary>
		/// Actives a check when any user arguments Provided via any overload that takes an 
		/// <c>params object[]</c>
		/// that should be used for Factory Injections. If enabled and Factory arguments are provided but could not match to any Factory an
		/// <c>InvalidOperationExcpetion</c>
		/// will be thrown.
		/// Default is True
		/// </summary>
		public bool CheckFactoryArguments { get; set; }

		/// <summary>
		/// Activates Deadlock and Stackoverflow detection and Prevention.
		/// When an Stackoverflow inside any FactoryMethod is detected an other method for creating the statement is used as long as there are other options
		/// </summary>
		public bool Multipath { get; set; }

		/// <summary>
		/// For StackOverflow detection
		/// </summary>
		[ThreadStatic]
		private static bool _isIndented;

		internal IDbCommand CreateSelectQueryFactory(DbClassInfoCache type,
			params object[] parameter)
		{
			if (!parameter.Any())
			{
				if (type.SelectFactory != null)
				{
					return Database.CreateCommand(type.SelectFactory.Attribute.Query);
				}
			}

			return GenericQueryCreation<SelectFactoryMethodAttribute>(type, (e, f) => CreateSelect(type, Database), null, parameter);
		}

		internal IDbCommand CreateInsertQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<InsertFactoryMethodAttribute>(type, (e, f) => DbAccessLayer.CreateInsert(Database, type, e), entity);
		}

		internal IDbCommand CreateUpdateQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<UpdateFactoryMethodAttribute>(type, (e, f) => DbAccessLayer.CreateUpdate(Database, type, e), entity);
		}

		internal IDbCommand CreateDeleteQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<DeleteFactoryMethodAttribute>(type, (e, f) => DbAccessLayer.CreateDelete(Database, type, e), entity);
		}

		internal IDbCommand GenericQueryCreation<TE>(
			DbClassInfoCache type,
			Func<object, IDatabase, IDbCommand> fallback,
			object entity = null,
			params object[] parameter)
			where TE : DbAccessTypeAttribute
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (fallback == null)
				throw new ArgumentNullException("fallback");

			var factoryAttribute = typeof(TE);

			try
			{
				if (_isIndented)
				{
					if (Multipath)
					{
						return fallback(entity, Database);
					}
					else
					{
						throw new InvalidOperationException("This method is not allowed in the context of any FactoryMethod. Enable Multipath to allow the Intiligent Query creation");
					}
				}
				_isIndented = true;

				var arguments = parameter.ToList();

				//try to get the attribute for static selection
				if (!arguments.Any())
				{
					if (factoryAttribute == typeof(SelectFactoryMethodAttribute) && type.SelectFactory != null
						&& (!IsMultiProviderEnvironment || type.SelectFactory.Attribute.TargetDatabase == Database.TargetDatabase))
					{
						return DbAccessLayerHelper.CreateCommand(Database, type.SelectFactory.Attribute.Query);
					}
				}
				
				var methods =
					type.Mehtods
						.Where(s => s.Attributes.Any(e => e.Attribute is TE && (!IsMultiProviderEnvironment
							|| (e.Attribute as TE).TargetDatabase == Database.TargetDatabase)))
						.ToArray();

				if (methods.Any())
				{
					var searchMethodWithFittingParams = methods.Where(s =>
					{
						var parameterInfos = s.Arguments.Where(f => typeof(RootQuery) != f.Type).ToArray();

						if (parameterInfos.Length != arguments.Count)
							return false;

						for (var i = 0; i < parameterInfos.Length; i++)
						{
							var para = parameterInfos[i];
							if (para.ParameterInfo.IsOptional)
							{
								continue;
							}
							var tryParam = arguments[i];
							if (tryParam == null)
								return false;
							if (!(para.Type == tryParam.GetType()))
							{
								return false;
							}
						}
						return true;
					}).ToArray();

					if (searchMethodWithFittingParams.Length != 1)
					{
						if (this.CheckFactoryArguments && arguments.Any())
						{
							ThrowNoFactoryFoundException<TE>(arguments);
						}
						return fallback(entity, Database);
					}

					var method = searchMethodWithFittingParams.First();
					//must be public static if its an Select otherwise it has to be an instance member
					if (
						(factoryAttribute == typeof(SelectFactoryMethodAttribute)
						&& !method.MethodInfo.IsStatic)
						|| (factoryAttribute != typeof(SelectFactoryMethodAttribute)
						&& method.MethodInfo.IsStatic))
					{
						if (this.CheckFactoryArguments && arguments.Any())
						{
							ThrowNoFactoryFoundException<TE>(arguments);
						}
						return fallback(entity, Database);
					}

					var cleanParams = arguments.Any() ? arguments : null;
					var dbMethodArgument = method.Arguments.FirstOrDefault();
					IQueryBuilder queryBuilder = null;
					if (dbMethodArgument != null && dbMethodArgument.Type == typeof(RootQuery))
					{
						queryBuilder = Query();
						if (cleanParams == null)
							cleanParams = new List<object>();

						cleanParams.Insert(0, queryBuilder);
					}

					object invoke;
					if (cleanParams != null)
					{
						invoke = method.Invoke(entity, cleanParams.ToArray());
					}
					else
					{
						invoke = method.Invoke(entity);
					}
					if (invoke != null)
					{
						if (invoke is string && !string.IsNullOrEmpty(invoke as string))
						{
							return DbAccessLayerHelper.CreateCommand(Database, invoke as string);
						}
						if (invoke is IQueryFactoryResult)
						{
							var result = invoke as IQueryFactoryResult;
							return Database.CreateCommandWithParameterValues(result.Query, result.Parameters);
						}
					}
					else if (queryBuilder != null)
					{
						return queryBuilder.ContainerObject.Compile();
					}
				}
				if (this.CheckFactoryArguments && arguments.Any())
				{
					ThrowNoFactoryFoundException<TE>(arguments);
				}
				return fallback(entity, Database);
			}
			finally
			{
				_isIndented = false;
			}
		}

		private static void ThrowNoFactoryFoundException<TE>(List<object> arguments) 
			where TE : DbAccessTypeAttribute
		{
			var invalidOperationException = new InvalidOperationException("CheckFactoryArguments is activated and arguments are provided but no factory machtes the given arguments");
			var types = new List<string>();
			foreach (var argument in arguments)
			{
				types.Add(argument.GetType().ToString());
			}

			var data = new List<string>();
			foreach (var argument in arguments)
			{
				data.Add(argument.ToString());
			}
			invalidOperationException.Data.Add("FactoryType", typeof(TE));
			invalidOperationException.Data.Add("Types", types);
			invalidOperationException.Data.Add("Data", data);
			throw invalidOperationException;
		}


		/// <summary>
		///     Creates a Select for one Item with appended query and inclueded QueryCommand Paramater
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateSelect(Type type, object pk)
		{
			if (this.GetClassInfo(type).PrimaryKeyProperty == null)
				throw new NotSupportedException(string.Format("Class '{0}' does not define any Primary key", type.Name));

			var query = CreateSelectQueryFactory(this.GetClassInfo(type)).CommandText
				+ " WHERE " + this.GetClassInfo(type).PrimaryKeyProperty.DbName + " = @pk";
			var cmd = Database.CreateCommand(query);
			cmd.Parameters.AddWithValue("@pk", pk, Database);
			return cmd;
		}

		/// <summary>
		///     Creates a Select for one Item with appended query and inclueded QueryCommand Paramater
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDbCommand CreateSelect<T>(object pk)
		{
			return CreateSelect(typeof(T), pk);
		}

		/// <summary>
		///     Creates a Plain Select statement by using
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public string CreateSelect(Type type)
		{
			return CreateSelect(GetClassInfo(type));
		}
		
		/// <summary>
		///     Creates a Plain Select statement by using
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static string CreateSelect(DbClassInfoCache classType, string prefix = null)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			if (prefix != null)
				sb.Append(prefix + " ");
			sb.Append(classType.CreatePropertyCsv(
				classType
					.Propertys
					.Where(f => f.Value.ForginKeyAttribute != null ||
						(
							f.Value.FromXmlAttribute != null
							&& f.Value.FromXmlAttribute.Attribute.LoadStrategy == LoadStrategy.NotIncludeInSelect
						))
					.Select(f => f.Key)
					.ToArray()));
			sb.Append(" FROM ");
			sb.Append(classType.TableName);
			return sb.ToString();
		}

		/// <summary>
		///     Creates a Select by using a Factory mehtod or auto generated querys
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDbCommand CreateSelect<T>()
		{
			return CreateSelect(Config.GetOrCreateClassInfoCache(typeof(T)), Database);
		}

		/// <summary>
		///     Creates a Select by using a Factory mehtod or auto generated querys
		/// </summary>
		/// <returns></returns>
		private static IDbCommand CreateSelect(DbClassInfoCache type, IDatabase db)
		{
			return db.CreateCommand(CreateSelect(type));
		}

		#endregion

		#region Runs

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public IEnumerable RunDynamicSelect(Type type, IDbCommand query)
		{
			RaiseSelect(query);
			var typeInfo = type.GetClassInfo();
			return EnumerateDataRecords(query, this.LoadCompleteResultBeforeMapping, typeInfo);

			//if (egarLoading)
			//{
			//	var recordToNameMapping = new Dictionary<int, DbPropertyInfoCache>();

			//	if (!results.Any())
			//		return new ArrayList();

			//	var anyReader = results.First();

			//	for (var i = 0; i < anyReader.FieldCount; i++)
			//	{
			//		DbPropertyInfoCache val = null;
			//		typeInfo.Propertys.TryGetValue(typeInfo.SchemaMappingDatabaseToLocal(anyReader.GetName(i)),
			//			out val);
			//		recordToNameMapping.Add(i, val);
			//	}

			//	return
			//		results
			//			.Select(record => SetPropertysViaReflection(typeInfo, record, recordToNameMapping))
			//			.ToArray();
			//}
			//return EnumerateDirectDataRecords(query, typeInfo);
		}

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] RunSelect(Type type, IDbCommand query)
		{
			return RunDynamicSelect(type, query).Cast<object>().ToArray();
		}

		/// <summary>
		///     Executes a Selectstatement and Parse the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] RunSelect<T>(IDbCommand query)
		{
			return RunSelect(typeof(T), query).Cast<T>().ToArray();
		}

		/// <summary>
		///     Executes
		///     <paramref name="query" />
		///     and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] RunSelect(Type type, string query,
			IEnumerable<IQueryParameter> paramenter)
		{
			return
				Database.Run(
					s =>
					{
						var command = DbAccessLayerHelper.CreateCommand(s, query);

						foreach (IQueryParameter item in paramenter)
							command.Parameters.AddWithValue(item.Name, item.Value, s);
						return RunSelect(type, command);
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
		public T[] RunSelect<T>(string query, IEnumerable<IQueryParameter> paramenter)
		{
			return RunSelect(typeof(T), query, paramenter).Cast<T>().ToArray();
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
			return SelectWhere(type, where, null);
		}

		/// <summary>
		///     Executes a Select Statement and adds
		///     <paramref name="where" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] SelectWhere<T>(String @where)
		{
			return SelectWhere(typeof(T), @where).Cast<T>().ToArray();
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
			if (!@where.StartsWith("WHERE"))
			{
				@where = @where.Insert(0, "WHERE ");
			}

			var query = CreateSelect(type, @where, paramenter);
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
			return SelectWhere(typeof(T), where, paramenter).Cast<T>().ToArray();
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
			object[] selectWhere = SelectWhere(typeof(T), @where, paramenter);
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
			RaiseSelect(command);
			return EnumerateDataRecords(command).Select(s => s[0]).ToArray();
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
			var runPrimetivSelect = RunPrimetivSelect(typeof(T), query, enumarateFromDynamics);
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
			return RunPrimetivSelect(typeof(T), query, parameters).Cast<T>().ToArray();
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
		public T[] SelectNative<T>(string query)
		{
			return SelectNative(typeof(T), query).Cast<T>().ToArray();
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
			return SelectNative(type, DbAccessLayerHelper.CreateCommand(Database, query));
		}


		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public object[] SelectNative(Type type, IDbCommand command, bool multiRow)
		{
			if (!multiRow)
				return SelectNative(type, command);

			//var guessingRelations = new Dictionary<PropertyInfo, IDbCommand>();

			//var propertyInfos = type.GetFKs();
			//var primaryKeyName = type.GetPK();

			//foreach (var propertyInfo in propertyInfos)
			//{
			//    guessingRelations.Append(propertyInfo, database.CreateCommand(string.Format("JOIN {0} ON {0} = {1}", propertyInfo.DeclaringType.GetTableName(), primaryKeyName)));
			//}

			/*
			 * Due the fact that you are not able to anylse the QueryCommand in a way to ensure its will not effect the query self we
			 * are loading the result an then loading based on that the items             
			 */

			var sel = RunSelect(type, command);

			if (ProcessNavigationPropertys && this.GetClassInfo(type).HasRelations)
			{
				return sel.AsParallel().Select(s => LoadNavigationProps(s)).ToArray();
			}

			return sel;
		}

		/// <summary>
		///     ToBeSupported
		/// </summary>
		/// <returns></returns>
		public object LoadNavigationProps(object source)
		{
			//Get nav Propertys
			foreach (var propertyInfo in source.GetType().GetNavigationProps(this.Config))
			{
				//var firstOrDefault = source.GetFK<long>(propertyInfo.ClassName);
				IDbCommand sqlCommand;

				var firstOrDefault =
					propertyInfo.GetCustomAttributes().FirstOrDefault(s => s is ForeignKeyAttribute) as
						ForeignKeyAttribute;
				if (firstOrDefault == null)
					continue;
				Type targetType = null;
				if (propertyInfo.CheckForListInterface())
				{
					object pk = source.GetPK(this.Config);
					var targetName = firstOrDefault.KeyName;
					targetType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();

					if (String.IsNullOrEmpty(targetName))
					{
						targetName = targetType.GetPK(this.Config);
					}

					sqlCommand = CreateSelect(targetType,
						" WHERE " + targetName + " = @pk", new List<IQueryParameter>
						{
							new QueryParameter("@pk", pk)
						});
				}
				else
				{
					object fkproperty = source.GetParamaterValue(this.Config, firstOrDefault.KeyName);

					if (fkproperty == null)
						continue;

					targetType = propertyInfo.PropertyType;
					sqlCommand = CreateSelect(targetType, fkproperty);
				}

				var orDefault = RunSelect(targetType, sqlCommand);

				//result is list and property is list
				if (orDefault.CheckForListInterface() && propertyInfo.CheckForListInterface())
				{
					var constructorInfo =
						typeof(DbCollection<>).MakeGenericType(targetType).GetConstructor(new[] { typeof(IEnumerable) });

					var reproCollection = constructorInfo.Invoke(new object[] { orDefault });
					propertyInfo.Setter.Invoke(source, reproCollection);
					foreach (object item in orDefault)
						LoadNavigationProps(item);
				}
				if (propertyInfo.CheckForListInterface())
					continue;

				var @default = orDefault.FirstOrDefault();
				propertyInfo.Setter.Invoke(source, @default);
				LoadNavigationProps(@default);
			}

			return source;
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
			var objects = RunSelect(type, command);

			if (ProcessNavigationPropertys && this.GetClassInfo(type).HasRelations)
				foreach (object model in objects)
					LoadNavigationProps(model);

			return objects.ToArray();
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
			return RunSelect<T>(query, paramenter);
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
			var objects = (object[])SelectNative(typeof(T), query, paramenter);
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
			var mars = EnumerateMarsDataRecords(bulk);
			var concatedMarsToType = new List<Tuple<DbClassInfoCache, List<IDataRecord>>>();
			for (var index = 0; index < mars.Count; index++)
			{
				var dataRecord = mars[index];
				var expectedResult = marsTypes[index];
				concatedMarsToType.Add(new Tuple<DbClassInfoCache, List<IDataRecord>>(this.GetClassInfo(expectedResult), dataRecord));
			}
			var list =
				concatedMarsToType.Select(s => s.Item2.Select(e => SetPropertysViaReflection(s.Item1, e)).AsParallel().ToList())
					.AsParallel()
					.ToList();
			return list;
		}
	}
}