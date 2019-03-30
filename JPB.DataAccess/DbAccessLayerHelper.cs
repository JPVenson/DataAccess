#region usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess
{
	/// <summary>
	/// </summary>
	public static class DbAccessLayerHelper
	{
		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(
			this DbClassInfoCache type,
			EagarDataRecord reader,
			DbAccessType? accessType = null,
			DbConfig config = null)
		{
			if (reader == null)
			{
				return null;
			}

			bool created;
			var source = DbAccessLayer.CreateInstance(type, reader, out created);
			if (created)
			{
				return source;
			}

			if (config == null)
			{
				config = new DbConfig(true);
			}

#pragma warning disable 618
			return DbAccessLayer.ReflectionPropertySet(config, source, type, reader, new DbAccessLayer.ReflectionSetCacheModel(), accessType);
#pragma warning restore 618
		}
		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(
			this DbClassInfoCache type,
			XmlDataRecord reader,
			DbAccessType? accessType = null,
			DbConfig config = null)
		{
			if (reader == null)
			{
				return null;
			}

			var eagerReader = new EagarDataRecord();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				eagerReader.Add(eagerReader.GetName(i), eagerReader[i]);
			}

			bool created;
			var source = DbAccessLayer.CreateInstance(type, eagerReader, out created);
			if (created)
			{
				return source;
			}

			if (config == null)
			{
				config = new DbConfig(true);
			}

#pragma warning disable 618
			return DbAccessLayer.ReflectionPropertySet(config, source, type, eagerReader, new DbAccessLayer.ReflectionSetCacheModel(), accessType);
#pragma warning restore 618
		}

		/// <summary>
		///     Not Connection save
		///     Must be executed inside a Valid Connection
		/// </summary>
		/// <param name="db"></param>
		/// <param name="base">left part of the query</param>
		/// <param name="last">right part of the query</param>
		/// <param name="autoRename">
		///     If an conflict happens a renaming operation happens(use this argument if you are done with the
		///     query generation otherwise querys might be invalid) if an collision is detected and this is false an exception will
		///     be thrown
		/// </param>
		/// <param name="delimiter">Delimiter for separation of both commands</param>
		/// <returns></returns>
		public static IDbCommand MergeCommands(this IDatabase db, IDbCommand @base, IDbCommand last,
			bool autoRename = false, string delimiter = "; ")
		{
			return db.MergeTextToParameters(new[] {@base, last}, autoRename, 1, true, delimiter);
		}

		internal static IDbCommand AppendSuffix(this IDatabase db, IDbCommand left, string suffix)
		{
			var commandText = left.CommandText;
			var parameter = new List<QueryParameter>();
			foreach (var param in left.Parameters.Cast<IDataParameter>())
			{
				commandText = commandText.Replace(param.ParameterName, param.ParameterName + suffix);
				parameter.Add(new QueryParameter(param.ParameterName + suffix, param.Value, param.DbType));
			}
			return db.CreateCommandWithParameterValues(commandText, parameter);
		}

		/// <summary>
		///		Merges all Queries together
		/// </summary>
		/// <param name="autoRename"></param>
		/// <param name="seed"></param>
		/// <param name="pessimistic"></param>
		/// <param name="insertDelimiter"></param>
		/// <param name="others"></param>
		/// <returns></returns>
		public static IQueryFactoryResult MergeQueryFactoryResult(bool autoRename = false,
			int seed = 1,
			bool pessimistic = true,
			string insertDelimiter = null,
			params IQueryFactoryResult[] others)
		{
			var commandText = new StringBuilder();
			var parameter = new List<IQueryParameter>();

			if (pessimistic)
			{
				foreach (var dbCommand in others)
				{
					var commandTextOfChild = new StringBuilder(dbCommand.Query);
					foreach (var item in dbCommand.Parameters)
					{
						if (parameter.Any(s => s.Name == item.Name))
						{
							//Parameter is found twice in both commands so rename it
							if (!autoRename)
							{
								throw new ArgumentOutOfRangeException(nameof(others),
									string.Format("The parameter {0} exists twice. Allow Auto renaming or change one of the commands",
										item.Name));
							}
							var counter = seed;
							var parameterName = item.Name;
							var buffParam = parameterName;
							while (parameter.Any(s => s.Name == buffParam))
							{
								buffParam = string.Format("{0}_{1}", parameterName, counter);
								counter++;
							}
							commandTextOfChild = commandTextOfChild.Replace(item.Name, buffParam);
							item.Name = buffParam;
						}

						parameter.Add(item);
					}

					commandText.Append((insertDelimiter ?? " ") + commandTextOfChild);
				}
			}
			else
			{
				foreach (var dbCommand in others)
				{
					parameter.AddRange(dbCommand.Parameters);
					commandText.Append((insertDelimiter ?? " ") + dbCommand.Query);
				}
			}

			return new QueryFactoryResult(commandText.ToString(), parameter.ToArray());
		}

		/// <summary>
		///     Merges 2 Commands into one single New Command by optionaly renaming and Delimiter insert
		/// </summary>
		/// <param name="db"></param>
		/// <param name="commmands">The right part of the query</param>
		/// <param name="autoRename">
		///     if the merge will find some conflics in arguments, shout it provide a new name or throw an
		///     extention
		///     <value>true</value>
		///     Rename and continue
		///     <value>fale</value>
		///     throw an exception
		/// </param>
		/// <param name="seed">For optimation</param>
		/// <param name="pessimistic">
		///     <value>true</value>
		///     you will expect that there are conflics
		///     <value>false</value>
		///     you expect no conflics
		/// </param>
		/// <param name="insertDelimiter">insert the SQL Delimiter between base and last or not</param>
		/// <returns></returns>
		public static IDbCommand MergeTextToParameters(this IDatabase db,
			IDbCommand[] commmands,
			bool autoRename = false,
			int seed = 1,
			bool pessimistic = true,
			string insertDelimiter = null)
		{
			var commandText = new StringBuilder();
			var parameter = new List<IQueryParameter>();

				//@base.Parameters.Cast<IDataParameter>()
				//	.Select(item => new QueryParameter(item.ParameterName, item.Value, item.DbType))
				//	.Cast<IQueryParameter>()
				//	.ToList();

			if (pessimistic)
			{
				foreach (var dbCommand in commmands)
				{
					foreach (var item in dbCommand.Parameters.Cast<IDataParameter>())
					{
						if (parameter.Any(s => s.Name == item.ParameterName))
						{
							//Parameter is found twice in both commands so rename it
							if (!autoRename)
							{
								throw new ArgumentOutOfRangeException(nameof(commmands),
									string.Format("The parameter {0} exists twice. Allow Auto renaming or change one of the commands",
										item.ParameterName));
							}
							var counter = seed;
							var parameterName = item.ParameterName;
							var buffParam = parameterName;
							while (parameter.Any(s => s.Name == buffParam))
							{
								buffParam = string.Format("{0}_{1}", parameterName, counter);
								counter++;
							}
							dbCommand.CommandText = dbCommand.CommandText.Replace(item.ParameterName, buffParam);

							item.ParameterName = buffParam;
						}

						parameter.Add(new QueryParameter(item.ParameterName, item.Value, item.DbType));
					}

					commandText.Append((insertDelimiter ?? " ") + dbCommand.CommandText);
				}
			}
			else
			{
				foreach (var dbCommand in commmands)
				{
#pragma warning disable 618
					parameter.AddRange(dbCommand.Parameters.AsQueryParameter());
#pragma warning restore 618
					commandText.Append((insertDelimiter ?? " ") + dbCommand.CommandText);
				}
			}
			
			return db.CreateCommandWithParameterValues(commandText.ToString(), parameter);
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
				from IDataParameter parameter in source
				select new QueryParameter(parameter.ParameterName, parameter.Value);
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
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, DbClassInfoCache type, string query,
			string[] propertyInfos, object entry)
		{
			var propertyvalues =
				propertyInfos.Select(
					propertyInfo =>
					{
						DbPropertyInfoCache property;
						type.Propertys.TryGetValue(propertyInfo, out property);
						var val = property.GetConvertedValue(entry);
						var dataValue = val ?? DBNull.Value;
						return new Tuple<Type, object>(property.PropertyType, dataValue);
					}).ToArray();
			return db.CreateCommandWithParameterValues(query, propertyvalues);
		}

		/// <summary>
		///     Wrappes a String into a Command
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommand(this string commandText, IDatabase db, object param = null)
		{
			return db.CreateCommand(commandText, EnumerateFromUnknownParameter(param).FromUserDefinedToSystemParameters(db));
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
		public static IDbCommand CreateCommandWithParameterValues<T>(this DbAccessLayer db, string query,
			string[] propertyInfos,
			T entry)
		{
			return db.Database.CreateCommandWithParameterValues(db.GetClassInfo(typeof(T)), query, propertyInfos, entry);
		}

		/// <summary>
		///     Wraps
		///     <paramref name="query" />
		///     into a Command and adds the values
		///     values are added by Index
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query,
			params Tuple<Type, object>[] values)
		{
			var listofQueryParamter = new List<IQueryParameter>();
			for (var i = 0; i < values.Count(); i++)
			{
				listofQueryParamter.Add(new QueryParameter(i.ToString(CultureInfo.InvariantCulture), values[i].Item2)
				{
					SourceType = values[i].Item1
				});
			}
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
			{
				return cmd;
			}
			foreach (var queryParameter in values)
			{
				var dbDataParameter = cmd.CreateParameter();
				dbDataParameter.DbType = queryParameter.SourceDbType;
				dbDataParameter.Value = queryParameter.Value;
				dbDataParameter.ParameterName = queryParameter.Name.CheckParamter();
				cmd.Parameters.Add(dbDataParameter);
			}

			db.LastExecutedQuery?.Refresh();
			return cmd;
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
			IEnumerable<IDataParameter> values)
		{
			var cmd = CreateCommand(db, query);
			if (values == null)
			{
				return cmd;
			}
			foreach (var queryParameter in values)
			{
				var dbDataParameter = cmd.CreateParameter();
				dbDataParameter.DbType = queryParameter.DbType;
				dbDataParameter.Value = queryParameter.Value;
				dbDataParameter.ParameterName = queryParameter.ParameterName;
				cmd.Parameters.Add(dbDataParameter);
			}

			db.LastExecutedQuery?.Refresh();
			return cmd;
		}

		internal static IEnumerable<IQueryParameter> EnumerateFromUnknownParameter(this object parameter)
		{
			if (parameter == null)
			{
				return new IQueryParameter[0];
			}

			if (parameter is IQueryParameter)
			{
				return new[] { parameter as IQueryParameter };
			}

			if (parameter is IEnumerable<IQueryParameter>)
			{
				return parameter as IEnumerable<IQueryParameter>;
			}

			using (var dbConfig = new DbConfig(true))
			{
				var dynType = dbConfig.GetOrCreateClassInfoCache(parameter.GetType());
				var elements = new List<IQueryParameter>();
				foreach (var dynProperty in dynType.Propertys)
				{
					var convertedParam = parameter.GetParameterValue(dbConfig, dynProperty.Key);
					elements.Add(new QueryParameter(dynProperty.Key.CheckParamter(), convertedParam));
				}
				return elements;
			}
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection
		/// </summary>
		/// <returns></returns>
		public static string CreatePropertyCsv(this DbClassInfoCache type, bool ignorePk = false)
		{
			return CreatePropertyNames(type, ignorePk).Aggregate((e, f) => e + ", " + f);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static string CreatePropertyCsv<T>(DbConfig config, bool ignorePk = false)
		{
			return CreatePropertyCsv(config.GetOrCreateClassInfoCache(typeof(T)), ignorePk);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableAlias"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		internal static string ColumnIdentifier(string tableAlias, string columnName)
		{
			var col = $"[{columnName.TrimAlias()}]";
			if (tableAlias != null)
			{
				return $"[{tableAlias.TrimAlias()}].{col}";
			}
			return col;
		}


		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv(this DbClassInfoCache type, string alias, params string[] ignore)
		{
			var properties = CreateProperties(type, alias, ignore);
			if (properties.Any())
			{
				return properties.Aggregate((e, f) => e + ", " + f);
			}

			return "";
		}

		internal static string[] CreateProperties(this DbClassInfoCache type, string alias, params string[] ignore)
		{
			var filteredList = FilterDbSchemaMapping(type, ignore).ToArray();

			if (filteredList.Any())
			{
				return filteredList.Select(e => ColumnIdentifier(alias, e)).ToArray();
			}
			return new string[0];
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv<T>(DbConfig config, string alias, params string[] ignore)
		{
			return CreatePropertyCsv(config.GetOrCreateClassInfoCache(typeof(T)), alias, ignore);
		}

		/// <summary>
		///     Maps all propertys of
		///     <paramref name="type" />
		///     into the Database columns
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<string> FilterDbSchemaMapping(this DbClassInfoCache type, params string[] ignore)
		{
			return type.LocalToDbSchemaMapping().Where(f => !ignore.Contains(f));
		}

		/// <summary>
		///     Maps all propertys of typeof(T) into the Database columns
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static IEnumerable<string> FilterDbSchemaMapping<T>(DbConfig config, params string[] ignore)
		{
			return FilterDbSchemaMapping(config.GetOrCreateClassInfoCache(typeof(T)), ignore);
		}


		/// <summary>
		///     Maps propertys to database of given type
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<string> CreatePropertyNames(DbClassInfoCache type, bool ignorePk = false)
		{
			return ignorePk ? FilterDbSchemaMapping(type, type.PrimaryKeyProperty.DbName) : FilterDbSchemaMapping(type);
		}

		/// <summary>
		///     Maps propertys to database of given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static IEnumerable<string> CreatePropertyNames<T>(DbConfig config, bool ignorePk = false)
		{
			return ignorePk ? FilterDbSchemaMapping<T>(config, typeof(T).GetPK(config)) : FilterDbSchemaMapping<T>(config);
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
		///     Execute a QueryCommand on a given Database
		/// </summary>
		/// <returns></returns>
		public static int ExecuteGenericCommand(this IDbCommand command, IDatabase db)
		{
			return db.Run(s => s.ExecuteNonQuery(command));
		}

		/// <summary>
		///     Not Connection save
		///     Must be executed inside a Valid Connection
		///     Takes <paramref name="base" /> as base of Connection properties
		///     Merges the Command text of Both commands separated by a space
		///     Creates a new command based on
		///     <paramref name="db" />
		///     and Adds the Merged CommandText and all parameter to it
		/// </summary>
		/// <returns></returns>
		public static IDbCommand ConcatCommands(IDatabase db, IDbCommand @base, IDbCommand last, bool autoRename = false)
		{
			return db.MergeTextToParameters(new[] { @base, last }, autoRename);
		}

		/// <summary>
		///     Not Connection save
		///     Must be executed inside a Valid Connection
		///     Merges the Command text of all commands separated by a space
		///     Creates a new command based on
		///     <paramref name="db" />
		///     and Adds the Merged CommandText and all parameter to it
		/// </summary>
		/// <returns></returns>
		public static IDbCommand ConcatCommands(IDatabase db, bool autoRename, params IDbCommand[] commands)
		{
			return db.MergeTextToParameters(commands, autoRename);
		}
	}
}