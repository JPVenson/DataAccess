/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
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
					.Select(item => new QueryParameter(item.ParameterName, item.Value, item.DbType))
					.Cast<IQueryParameter>()
					.ToList();

			var commandText = last.CommandText;

			foreach (var item in last.Parameters.Cast<IDataParameter>())
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

				parameter.Add(new QueryParameter(item.ParameterName, item.Value, item.DbType));
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
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, Type type, string query,
			string[] propertyInfos, object entry)
		{
			var classInfo = type.GetClassInfo();
			var propertyvalues =
				propertyInfos.Select(
					propertyInfo =>
					{
						DbPropertyInfoCache property;
						classInfo.Propertys.TryGetValue(propertyInfo, out property);
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
		public static IDbCommand CreateCommandWithParameterValues(this IDatabase db, string query,
			params Tuple<Type, object>[] values)
		{
			var listofQueryParamter = new List<IQueryParameter>();
			for (var i = 0; i < values.Count(); i++)
				listofQueryParamter.Add(new QueryParameter(i.ToString(CultureInfo.InvariantCulture), values[i].Item2)
				{
					SourceType = values[i].Item1
				});
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
			foreach (var queryParameter in values)
			{
				var dbDataParameter = cmd.CreateParameter();
				dbDataParameter.DbType = queryParameter.SourceDbType;
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

			return (from element in parameter.GetType().GetClassInfo().Propertys
				let value = parameter.GetParamaterValue(element.Key)
				select new QueryParameter(element.Key.CheckParamter(), value)).Cast<IQueryParameter>()
				.ToList();
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
		public static string CreatePropertyCsv<T>(bool ignorePk = false)
		{
			return CreatePropertyCsv(typeof (T).GetClassInfo(), ignorePk);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv(this DbClassInfoCache type, params string[] ignore)
		{
			return FilterDbSchemaMapping(type, ignore).Aggregate((e, f) => e + ", " + f);
		}

		/// <summary>
		///     Returns all Propertys that can be loaded due reflection and excludes all propertys in ignore
		/// </summary>
		/// <returns></returns>
		internal static string CreatePropertyCsv<T>(params string[] ignore)
		{
			return CreatePropertyCsv(typeof (T).GetClassInfo(), ignore);
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
		internal static IEnumerable<string> FilterDbSchemaMapping<T>(params string[] ignore)
		{
			return FilterDbSchemaMapping(typeof (T).GetClassInfo(), ignore);
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
		internal static IEnumerable<string> CreatePropertyNames<T>(bool ignorePk = false)
		{
			return ignorePk ? FilterDbSchemaMapping<T>(typeof (T).GetPK()) : FilterDbSchemaMapping<T>();
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
		///     Takes <paramref name="base" /> as base of Connection propertys
		///     Merges the Command text of Both commands sepperated by a space
		///     Creats a new command based on
		///     <paramref name="db" />
		///     and Adds the Merged Commandtext and all parameter to it
		/// </summary>
		/// <returns></returns>
		public static IDbCommand ConcatCommands(IDatabase db, IDbCommand @base, IDbCommand last, bool autoRename = false)
		{
			return db.MergeTextToParameters(@base, last, autoRename);
		}

		/// <summary>
		///     Not Connection save
		///     Must be executed inside a Valid Connection
		///     Takes <paramref name="base" /> as base of Connection propertys
		///     Merges the Command text of Both commands sepperated by a space
		///     Creats a new command based on
		///     <paramref name="db" />
		///     and Adds the Merged Commandtext and all parameter to it
		/// </summary>
		/// <returns></returns>
		public static IDbCommand InsertCommands(IDatabase db, IDbCommand @base, IDbCommand toInsert, bool autoRename = false)
		{
			throw new NotSupportedException();
			//var mergedCommandText = string.Format(@base.CommandText, toInsert);
			//return db.MergeTextToParameters(mergedCommandText, @base, toInsert, autoRename);
		}

		internal static IDatabaseStrategy GenerateStrategy(this string fullValidIdentifyer, string connection)
		{
			if (string.IsNullOrEmpty(fullValidIdentifyer))
				throw new ArgumentException("Type was not found");

			var type = Type.GetType(fullValidIdentifyer);
			if (type == null)
			{
				var parallelQuery = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll",
					SearchOption.TopDirectoryOnly);

				//Assembly assam = null;

				Parallel.ForEach(parallelQuery, (s, e) =>
				{
					Assembly loadFile;
					try
					{
						loadFile = Assembly.LoadFile(s);
					}
					catch (Exception)
					{
						return;
					}
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

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(this DbClassInfoCache type, IDataRecord reader)
		{
			return type.SetPropertysViaReflection(reader, null);
		}

		/// <summary>
		///     Creates a new Instance based on possible Ctor's and the given
		///     <paramref name="reader" />
		/// </summary>
		/// <returns></returns>
		public static object SetPropertysViaReflection(this DbClassInfoCache type, IDataRecord reader,
			DbAccessType? accessType)
		{
			if (reader == null)
				return null;

			bool created;
			var source = DbAccessLayer.CreateInstance(type, reader, out created);
			if (created)
				return source;

			return DbAccessLayer.ReflectionPropertySet(source, type, reader, null, accessType);
		}
	}
}