#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JPB.DataAccess.Framework.AdoWrapper;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.Operators;

#endregion

namespace JPB.DataAccess.Framework.Manager
{
	public partial class DbAccessLayer
	{
		/// <summary>
		///     Executes a IDbCommand that will return multibe result sets that will be parsed to the marsTypes in order they are
		///     provided
		/// </summary>
		/// <returns></returns>
		public List<List<object>> ExecuteMARS(IDbCommand bulk, params Type[] marsTypes)
		{
			var mars = EnumerateMarsDataRecords(bulk);
			var concatedMarsToType = new List<Tuple<DbClassInfoCache, List<EagarDataRecord>>>();
			for (var index = 0; index < mars.Count; index++)
			{
				var dataRecord = mars[index];
				var expectedResult = marsTypes[index];
				concatedMarsToType.Add(
					new Tuple<DbClassInfoCache, List<EagarDataRecord>>(GetClassInfo(expectedResult), dataRecord));
			}

			var list =
				concatedMarsToType.Select(s =>
						s.Item2.Select(e => SetPropertysViaReflection(s.Item1, e)).AsParallel().ToList())
					.AsParallel()
					.ToList();
			return list;
		}

		#region BasicCommands

		/// <summary>
		///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public object Select(Type type, object pk)
		{
			return AsyncHelper.WaitSingle(SelectAsync(type, pk));
		}

		/// <summary>
		///     Execute select on a database with a standard Where [Primary Key] = <paramref name="pk" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object> SelectAsync(Type type, object pk)
		{
			return await SelectSingleAsync(type, pk, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
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
		[PublicAPI]
		public async Task<T[]> SelectAsync<T>()
		{
			return (await SelectAsync(typeof(T))).Cast<T>().ToArray();
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public T SelectSingle<T>(object pk)
		{
			return AsyncHelper.WaitSingle(SelectSingleAsync<T>(pk));
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public async Task<T> SelectSingleAsync<T>(object pk)
		{
			return (T) await SelectAsync(typeof(T), pk);
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		protected object SelectSingle(Type type, object pk, bool egarLoading)
		{
			return AsyncHelper.WaitSingle(SelectSingleAsync(type, pk, egarLoading));
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		protected async Task<object> SelectSingleAsync(Type type, object pk, bool egarLoading)
		{
			return await Database.RunAsync(async d =>
				(await SelectNativeAsync(type, CreateSelect(type, pk), egarLoading)).FirstOrDefault());
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		protected T SelectSingle<T>(object pk, bool egarLoading)
		{
			return AsyncHelper.WaitSingle(SelectSingleAsync<T>(pk, egarLoading));
		}

		/// <summary>
		///     Selectes a Entry by its PrimaryKey
		///     Needs to define a PrimaryKey attribute inside
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		protected async Task<T> SelectSingleAsync<T>(object pk, bool egarLoading)
		{
			return (T) await SelectSingleAsync(typeof(T), pk, egarLoading);
		}

		/// <summary>
		///     Creates and Executes a Plain select over a
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public object[] Select(Type type, params object[] parameter)
		{
			return AsyncHelper.WaitSingle(SelectAsync(type, parameter));
		}

		/// <summary>
		///     Creates and Executes a Plain select over a
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object[]> SelectAsync(Type type, params object[] parameter)
		{
			return await SelectAsync(type, LoadCompleteResultBeforeMapping, parameter);
		}

		/// <summary>
		///     Uses a Factory method to Generate a new set of T
		///     When no Factory is found an Reflection based Factory is used
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public T[] Select<T>(object[] parameter)
		{
			return AsyncHelper.WaitSingle(SelectAsync<T>(parameter));
		}

		/// <summary>
		///     Uses a Factory method to Generate a new set of T
		///     When no Factory is found an Reflection based Factory is used
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public async Task<T[]> SelectAsync<T>(object[] parameter)
		{
			var objects = await SelectAsync(typeof(T), parameter);
			return objects.Cast<T>().ToArray();
		}

		/// <summary>
		///     Creates and Executes a SelectStatement for a given
		///     <paramref name="type" />
		///     by using the
		///     <paramref name="parameter" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		protected object[] Select(Type type, bool egarLoading, params object[] parameter)
		{
			return AsyncHelper.WaitSingle(SelectAsync(type, egarLoading, parameter));
		}

		/// <summary>
		///     Creates and Executes a SelectStatement for a given
		///     <paramref name="type" />
		///     by using the
		///     <paramref name="parameter" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		protected async Task<object[]> SelectAsync(Type type, bool egarLoading, params object[] parameter)
		{
			return await Database.RunAsync(async d =>
				await SelectNativeAsync(type, CreateSelectQueryFactory(GetClassInfo(type), parameter), egarLoading));
		}

		#endregion

		#region CreateCommands

		/// <summary>
		///     Actives a check when any user arguments Provided via any overload that takes an
		///     <c>params object[]</c>
		///     that should be used for Factory Injections. If enabled and Factory arguments are provided but could not match to
		///     any Factory an
		///     <c>InvalidOperationExcpetion</c>
		///     will be thrown.
		///     Default is True
		/// </summary>
		public bool CheckFactoryArguments { get; set; }

		/// <summary>
		///     Activates Deadlock and Stackoverflow detection and Prevention.
		///     When an Stackoverflow inside any FactoryMethod is detected an other method for creating the statement is used as
		///     long as there are other options
		///     Will might cause problems in Multithreaded enviorments
		/// </summary>
		public bool Multipath { get; set; }

		/// <summary>
		///     For StackOverflow detection
		/// </summary>
		private static readonly AsyncLocal<bool> IsIndented = new AsyncLocal<bool>();

		internal IDbCommand CreateSelectQueryFactory(DbClassInfoCache type, Func<IDbCommand> fallback,
			params object[] parameter)
		{
			if (!parameter.Any())
			{
				if (type.SelectFactory != null)
				{
					return Database.CreateCommand(type.SelectFactory.Attribute.Query);
				}
			}

			return GenericQueryCreation<SelectFactoryMethodAttribute>(type, (e, f) => fallback(), null,
				parameter);
		}

		internal IDbCommand CreateSelectQueryFactory(DbClassInfoCache type,
			params object[] parameter)
		{
			return CreateSelectQueryFactory(type, () => CreateSelect(type, Database), parameter);
		}

		internal IDbCommand CreateInsertQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<InsertFactoryMethodAttribute>(type, (e, f) => CreateInsert(Database, type, e),
				entity);
		}

		internal IDbCommand CreateUpdateQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<UpdateFactoryMethodAttribute>(type, (e, f) => CreateUpdate(Database, type, e),
				entity);
		}

		internal IDbCommand CreateDeleteQueryFactory(DbClassInfoCache type,
			object entity,
			params object[] parameter)
		{
			return GenericQueryCreation<DeleteFactoryMethodAttribute>(type, (e, f) => CreateDelete(Database, type, e),
				entity);
		}

		internal IDbCommand GenericQueryCreation<TE>(
			DbClassInfoCache type,
			Func<object, IDatabase, IDbCommand> fallback,
			object entity = null,
			params object[] parameter)
			where TE : DbAccessTypeAttribute
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (fallback == null)
			{
				throw new ArgumentNullException(nameof(fallback));
			}

			var factoryAttribute = typeof(TE);

			try
			{
				if (IsIndented.Value)
				{
					if (Multipath)
					{
						return fallback(entity, Database);
					}
					else
					{
						throw new InvalidOperationException(
							"This method is not allowed in the context of any FactoryMethod. Enable Multipath to allow the Intiligent Query creation");
					}
				}

				IsIndented.Value = true;

				var arguments = parameter.ToList();

				//try to get the attribute for static selection
				if (!arguments.Any())
				{
					if (factoryAttribute == typeof(SelectFactoryMethodAttribute) && type.SelectFactory != null
					                                                             && (!IsMultiProviderEnvironment ||
					                                                                 type.SelectFactory.Attribute
						                                                                 .TargetDatabase ==
					                                                                 Database.TargetDatabase))
					{
						return DbAccessLayerHelper.CreateCommand(Database, type.SelectFactory.Attribute.Query);
					}
				}

				var methods =
					type.Mehtods
						.Where(s => s.Attributes.Any(e => e.Attribute is TE && (!IsMultiProviderEnvironment
						                                                        ||
						                                                        ((TE) e.Attribute).TargetDatabase ==
						                                                        Database.TargetDatabase)))
						.ToArray();

				if (methods.Any())
				{
					var searchMethodWithFittingParams = methods.Where(s =>
					{
						var parameterInfos = s.Arguments.Where(f => typeof(RootQuery) != f.Type).ToArray();

						if (parameterInfos.Length != arguments.Count)
						{
							return false;
						}

						for (var i = 0; i < parameterInfos.Length; i++)
						{
							var para = parameterInfos[i];
							if (para.ParameterInfo.IsOptional)
							{
								continue;
							}

							var tryParam = arguments[i];
							if (tryParam == null)
							{
								return false;
							}

							if (!(para.Type == tryParam.GetType()))
							{
								return false;
							}
						}

						return true;
					}).ToArray();

					if (searchMethodWithFittingParams.Length != 1)
					{
						if (CheckFactoryArguments && arguments.Any())
						{
							ThrowNoFactoryFoundException<TE>(arguments);
						}

						return fallback(entity, Database);
					}

					var method = searchMethodWithFittingParams.First();
					//must be public static if its an Select otherwise it has to be an instance member
					if (factoryAttribute == typeof(SelectFactoryMethodAttribute)
					    && !method.MethodInfo.IsStatic
					    || factoryAttribute != typeof(SelectFactoryMethodAttribute)
					    && method.MethodInfo.IsStatic)
					{
						if (CheckFactoryArguments && arguments.Any())
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
						if (method.ReturnType != typeof(IQueryBuilder))
						{
							throw new InvalidOperationException(
								"The feature of Query Factorys that return a IQueryBuilder has been retired. Please return an instance of IQueryFactoryResult (WITHOUT ROOTQUERY AS ARGUMENT) instead");
						}

						queryBuilder = Query();
						if (cleanParams == null)
						{
							cleanParams = new List<object>();
						}

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
						if (invoke is IQueryBuilder)
						{
							throw new NotSupportedException(
								"The feature of Query Factorys that return a IQueryBuilder has been retired. Please return an instance of IQueryFactoryResult instead");
						}

						if (!string.IsNullOrEmpty(invoke as string))
						{
							return DbAccessLayerHelper.CreateCommand(Database, invoke as string);
						}

						if (invoke is IQueryFactoryResult)
						{
							var result = invoke as IQueryFactoryResult;
							return Database.CreateCommandWithParameterValues(result.Query, result.Parameters.ToArray());
						}
					}
				}

				if (CheckFactoryArguments && arguments.Any())
				{
					ThrowNoFactoryFoundException<TE>(arguments);
				}

				return fallback(entity, Database);
			}
			finally
			{
				IsIndented.Value = false;
			}
		}

		private static void ThrowNoFactoryFoundException<TE>(List<object> arguments)
			where TE : DbAccessTypeAttribute
		{
			var invalidOperationException =
				new InvalidOperationException(
					"CheckFactoryArguments is activated and arguments are provided but no factory matches the given arguments");
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
		///     Creates a Select for one Item with appended query and inclueded QueryCommand Paramater.
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateSelect(Type type, object pk)
		{
			if (GetClassInfo(type).PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Class '{0}' does not define any Primary key",
					type.Name));
			}

			var query = CreateSelectQueryFactory(GetClassInfo(type)).CommandText
			            + $" WHERE {GetClassInfo(type).PrimaryKeyProperty.DbName.EnsureAlias()} = @pk";
			var cmd = Database.CreateCommand(query);
			cmd.Parameters.AddWithValue("@pk", pk, Database);
			return cmd;
		}

		/// <summary>
		///     Creates a Plain Select statement by using
		///     <paramref name="classType" />
		/// </summary>
		/// <returns></returns>
		public static string CreateSelect(string source, DbClassInfoCache classType, string target)
		{
			return CreateSelectByColumns(source,
				GetSelectableColumnsOf(classType).Select(e => e.EnsureAlias()).Aggregate((e, f) => e + ", " + f), target);
		}

		/// <summary>
		///     Gets a list of all columns that are selectable
		/// </summary>
		/// <returns></returns>
		public static string[] GetSelectableColumnsOf(DbClassInfoCache classType)
		{
			return classType.CreateProperties(
				classType
					.Propertys
					.Where(f => f.Value.ForginKeyAttribute != null ||
					            f.Value.FromXmlAttribute != null
					            && f.Value.FromXmlAttribute.Attribute.LoadStrategy == LoadStrategy.NotIncludeInSelect)
					.Select(f => f.Key)
					.ToArray());
		}

		/// <summary>
		///     Creates a Plain Select statement by using
		///     <paramref name="source" />
		/// </summary>
		/// <returns></returns>
		public static string CreateSelectByColumns(string source, string columns, string modifier)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT ");
			if (modifier != null)
			{
				sb.Append(modifier + " ");
			}

			sb.Append(columns);
			sb.Append(" FROM ");
			sb.Append($"{source.EnsureAlias()} ");

			return sb.ToString();
		}

		/// <summary>
		///     Creates a Select by using a Factory mehtod or auto generated querys.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDbCommand CreateSelect<T>()
		{
			return CreateSelect(Config.GetOrCreateClassInfoCache(typeof(T)), Database);
		}

		/// <summary>
		///     Creates a Select by using a Factory mehtod or auto generated querys.
		/// </summary>
		/// <returns></returns>
		private static IDbCommand CreateSelect(DbClassInfoCache type, IDatabase db)
		{
			return db.CreateCommand(CreateSelect(type.TableName, type, null));
		}

		#endregion

		#region Runs

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <paramref name="type" />.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable RunDynamicSelect(Type type, IDbCommand query)
		{
			return AsyncHelper.WaitSingle(RunDynamicSelectAsync(type, query));
		}

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <paramref name="type" />.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<IEnumerable> RunDynamicSelectAsync(Type type, IDbCommand query)
		{
			RaiseSelect(query);
			var typeInfo = GetClassInfo(type);
			return await EnumerateDataRecordsAsync(query, LoadCompleteResultBeforeMapping, typeInfo,
				CommandBehavior.SingleResult);
		}

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object[]> RunSelectAsync(Type type, IDbCommand query)
		{
			return (await RunDynamicSelectAsync(type, query)).Cast<object>().ToArray();
		}

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public object[] RunSelect(Type type, IDbCommand query)
		{
			return AsyncHelper.WaitSingle(RunSelectAsync(type, query));
		}

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public async Task<T[]> RunSelectAsync<T>(IDbCommand query)
		{
			return (await RunSelectAsync(typeof(T), query)).Cast<T>().ToArray();
		}

		/// <summary>
		///     Executes a query and Parse the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[PublicAPI]
		public T[] RunSelect<T>(IDbCommand query)
		{
			return AsyncHelper.WaitSingle(RunSelectAsync<T>(query));
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
		[PublicAPI]
		public object[] ExecuteSelect(Type type, IDbCommand command)
		{
			return AsyncHelper.WaitSingle(ExecuteSelectAsync(type, command));
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses the first line of output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object[]> ExecuteSelectAsync(Type type, IDbCommand command)
		{
			RaiseSelect(command);
			return (await EnumerateDataRecordsAsync(command, LoadCompleteResultBeforeMapping, GetClassInfo(type),
				CommandBehavior.SingleResult)).ToArray();
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public object[] SelectNative(Type type, IDbCommand command, bool multiRow)
		{
			return AsyncHelper.WaitSingle(SelectNativeAsync(type, command, multiRow));
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object[]> SelectNativeAsync(Type type, IDbCommand command, bool multiRow)
		{
			if (!multiRow)
			{
				return await SelectNativeAsync(type, command);
			}

			var sel = await RunSelectAsync(type, command);
			return sel;
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public object[] SelectNative(Type type, IDbCommand command)
		{
			return AsyncHelper.WaitSingle(SelectNativeAsync(type, command));
		}

		/// <summary>
		///     Runs
		///     <paramref name="command" />
		///     and parses output into
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public async Task<object[]> SelectNativeAsync(Type type, IDbCommand command)
		{
			return (await RunSelectAsync(type, command)).ToArray();
		}

		#endregion
	}
}