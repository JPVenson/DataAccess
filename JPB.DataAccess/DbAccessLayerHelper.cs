using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess
{
	/// <summary>
	/// </summary>
	public static class DbAccessLayerHelper
	{
		/// <summary>
		///     Not Connection save
		///     Must be executed inside a Valid Connection
		/// </summary>
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
			var parameter =
				@base.Parameters.Cast<IDataParameter>()
					.Select(item => new QueryParameter {Name = item.ParameterName, Value = item.Value})
					.Cast<IQueryParameter>()
					.ToList();

			var commandText = last.CommandText;

			foreach (IDataParameter item in last.Parameters.Cast<IDataParameter>())
			{
				if (parameter.Any(s => s.Name == item.ParameterName))
				{
					//Parameter is found twice in both commands so rename it
					if (!autoRename)
					{
						throw new ArgumentOutOfRangeException("base",
							string.Format("The parameter {0} exists twice. Allow Auto renaming or change one of the commands",
								item.ParameterName));
					}
					var counter = 1;
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

				parameter.Add(new QueryParameter {Name = item.ParameterName, Value = item.Value});
			}


			return db.CreateCommandWithParameterValues(@base.CommandText + "; " + commandText, parameter);
		}

		/// <summary>
		///     Creates a DbCollection for the specifiy type
		///     To Limit the output create a new Type and then define the statement
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static DbCollection<T> CreateDbCollection<T>(this DbAccessLayer layer)
			where T : class,
				INotifyPropertyChanged
		{
			return new DbCollection<T>(layer.Select<T>());
		}

		/// <summary>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		[Obsolete("Not in use")]
		public static IEnumerable<IQueryParameter> AsQueryParameter(this IDataParameterCollection source)
		{
			return
				(from IDataParameter parameter in source
					select new QueryParameter(parameter.ParameterName, parameter.Value));
		}

		/// <summary>
		///     Wraps a
		///     <paramref name="query" />
		///     on a given
		///     <paramref name="type" />
		///     by including
		///     <paramref name="entry" />
		///     's
		///     propertys that are defined in
		///     <paramref name="propertyInfos" />
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, Type type, string query,
			string[] propertyInfos, object entry)
		{
			var classInfo = type.GetClassInfo();
			var propertyvalues =
				propertyInfos.Select(
					propertyInfo =>
					{
						DbPropertyInfoCache property;
						classInfo.PropertyInfoCaches.TryGetValue(propertyInfo, out property);
						var dataValue = DataConverterExtensions.GetDataValue(property.GetConvertedValue(entry));
						return dataValue;
					}).ToArray();
			return db.CreateCommandWithParameterValues(query, propertyvalues);
		}

		/// <summary>
		///     Wrappes a String into a Command
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommand(this string commandText, IDatabase db, object param = null)
		{
			return db.CreateCommand(commandText, EnumarateFromDynamics(param).FromUserDefinedToSystemParameters(db));
		}

		internal static IDataParameter[] FromUserDefinedToSystemParameters(this IEnumerable<IQueryParameter> parma,
			IDatabase db)
		{
			return parma.Select(s => db.CreateParameter(s.Name, s.Value)).ToArray();
		}

		/// <summary>
		///     Wraps a
		///     <paramref name="query" />
		///     on a given typeof(T) by including
		///     <paramref name="entry" />
		///     's
		///     propertys that are defined in
		///     <paramref name="propertyInfos" />
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommandWithParameterValues<T>(this IDatabase db, string query, string[] propertyInfos,
			T entry)
		{
			return db.CreateCommandWithParameterValues(typeof (T), query, propertyInfos, entry);
		}

		/// <summary>
		///     Wraps
		///     <paramref name="query" />
		///     into a Command and adds the values
		///     values are added by Index
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query, object[] values)
		{
			var listofQueryParamter = new List<IQueryParameter>();
			for (var i = 0; i < values.Count(); i++)
				listofQueryParamter.Add(new QueryParameter {Name = i.ToString(CultureInfo.InvariantCulture), Value = values[i]});
			return db.CreateCommandWithParameterValues(query, listofQueryParamter);
		}

		/// <summary>
		///     Wraps
		///     <paramref name="query" />
		///     into a Command and adds the values
		///     values are added by Name of IQueryParamter
		///     If item of
		///     <paramref name="values" />
		///     contains a name that does not contains @ it will be added
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query,
			IEnumerable<IQueryParameter> values)
		{
			var cmd = CreateCommand(db, query);
			if (values == null)
				return cmd;
			foreach (IQueryParameter queryParameter in values)
			{
				var dbDataParameter = cmd.CreateParameter();
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
				return new[] {parameter as IQueryParameter};
			}

			if (parameter is IEnumerable<IQueryParameter>)
			{
				return parameter as IEnumerable<IQueryParameter>;
			}

			return (from element in parameter.GetType().GetPropertiesEx()
				let value = parameter.GetParamaterValue(element.Name)
				select new QueryParameter {Name = element.Name.CheckParamter(), Value = value}).Cast<IQueryParameter>()
				.ToList();
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection
		/// </summary>
		/// <returns></returns>
		public static string CreatePropertyCsv(this Type type, bool ignorePk = false)
		{
			return CreatePropertyNames(type, ignorePk).Aggregate((e, f) => e + ", " + f);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static string CreatePropertyCsv<T>(bool ignorePk = false)
		{
			return CreatePropertyCsv(typeof (T), ignorePk);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv(this Type type, params string[] ignore)
		{
			return FilterDbSchemaMapping(type, ignore).Aggregate((e, f) => e + ", " + f);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv<T>(params string[] ignore)
		{
			return CreatePropertyCsv(typeof (T), ignore);
		}

		/// <summary>
		///     Maps all propertys of
		///     <paramref name="type" />
		///     into the Database columns
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<string> FilterDbSchemaMapping(this Type type, params string[] ignore)
		{
			return type.GetClassInfo().LocalToDbSchemaMapping().Where(f => !ignore.Contains(f));
		}

		/// <summary>
		///     Maps all propertys of typeof(T) into the Database columns
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static IEnumerable<string> FilterDbSchemaMapping<T>(params string[] ignore)
		{
			return FilterDbSchemaMapping(typeof (T), ignore);
		}

		internal static List<IDataRecord> EnumerateDataRecords(this IDatabase database, IDbCommand query, bool egarLoading)
		{
			return EnumerateMarsDataRecords(database, query, egarLoading).FirstOrDefault();
		}

		internal static List<List<IDataRecord>> EnumerateMarsDataRecords(this IDatabase database,
			IDbCommand query,
			bool egarLoading = true)
		{
			return database.Run(
				s =>
				{
					//Skip enumeration and parsing and make a Direct loading
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
									resultSet.Add(dr.CreateEgarRecord());
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

		internal static IEnumerable EnumerateDirectDataRecords(this IDatabase database, IDbCommand query,
			DbClassInfoCache info)
		{
			return database.Run(
				s =>
				{
					//Skip enumeration and parsing and make a Direct loading
					//This increeses Performance

					var records = new ArrayList();

					using (var dr = query.ExecuteReader())
					{
						try
						{
							do
							{
								while (dr.Read())
								{
									records.Add(info.SetPropertysViaReflection(dr));
								}
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
		///     Maps propertys to database of given type
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePk = false)
		{
			return ignorePk ? FilterDbSchemaMapping(type, type.GetPK()) : FilterDbSchemaMapping(type, new string[0]);
		}

		/// <summary>
		///     Maps propertys to database of given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static IEnumerable<string> CreatePropertyNames<T>(bool ignorePk = false)
		{
			return ignorePk ? FilterDbSchemaMapping<T>(typeof (T).GetPK()) : FilterDbSchemaMapping<T>(new string[0]);
		}

		/// <summary>
		///     Wraps a Parameterless string into a Command for the given DB
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommand(this IDatabase db, string query)
		{
			return db.CreateCommand(query);
		}


		/// <summary>
		///     Runs a Command on a given Database and Converts the Output into
		///     <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> ExecuteGenericCreateModelsCommand<T>(this IDbCommand command, IDatabase db)
			where T : class, new()
		{
			var info = typeof (T).GetClassInfo();
			return db.Run(
				s =>
					s.GetEntitiesList(command, info.SetPropertysViaReflection)
						.Cast<T>()
						.ToList());
		}

		/// <summary>
		///     Execute a Query on a given Database
		/// </summary>
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
				DbConfigHelper.GetMethods(type).Where(s => s.GetCustomAttributes(false).Any(e => e is TE)).ToArray();

			if (methods.Any())
			{
				var searchMethodWithFittingParams = methods.Where(s =>
				{
					var parameterInfos = s.GetParameters();

					if (parameterInfos.Length != param.Length)
					{
						return false;
					}

					for (var i = 0; i < parameterInfos.Length; i++)
					{
						var para = parameterInfos[i];
						var tryParam = param[i];
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
				if (typeof (TE) != typeof (SelectFactoryMethodAttribute)
				    || (typeof (TE) == typeof (SelectFactoryMethodAttribute) && method.IsStatic))
				{
					var cleanParams = param != null && param.Any() ? param : null;
					var invoke = method.Invoke(entry, cleanParams);
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
			return CreateCommandOfClassAttribute<TE>(type, entry, db, (o, database) => fallback((T) o, database), param);
		}

		internal static IDatabaseStrategy GenerateStrategy(this string fullValidIdentifyer, string connection)
		{
			if (String.IsNullOrEmpty(fullValidIdentifyer))
				throw new ArgumentException("Type was not found");

			var type = Type.GetType(fullValidIdentifyer);
			if (type == null)
			{
				var parallelQuery = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll",
					SearchOption.TopDirectoryOnly);

				//Assembly assam = null;

				Parallel.ForEach(parallelQuery, (s, e) =>
				{
					var loadFile = Assembly.LoadFile(s);
					var resolve = loadFile.GetType(fullValidIdentifyer);
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

			if (!typeof (IDatabaseStrategy).IsAssignableFrom(type))
			{
				throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");
			}

			//try constructor injection
			var ctOfType =
				type.GetConstructors()
					.FirstOrDefault(s => s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof (string));
			if (ctOfType != null)
			{
				return ctOfType.Invoke(new object[] {connection}) as IDatabaseStrategy;
			}
			var instanceOfType = Activator.CreateInstance(type) as IDatabaseStrategy;
			if (instanceOfType == null)
				throw new ArgumentException("Type was found but does not inhert from IDatabaseStrategy");

			instanceOfType.ConnectionString = connection;
			return instanceOfType;
		}
	}
}