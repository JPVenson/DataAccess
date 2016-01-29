using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
	partial class DbAccessLayer
	{
		/// <summary>
		///     get the size of the Partition of the singel InsertStatements
		/// </summary>
		public static uint RangerInsertPation { get; set; }

		private void DbAccessLayer_Insert()
		{
			RangerInsertPation = 25;
		}

		/// <summary>
		///     Insert a
		///     <paramref name="entry" />
		/// </summary>
		public void Insert(Type type, object entry)
		{
			Insert(type, entry, Database);
		}

		/// <summary>
		///     Insert a
		///     <paramref name="entry" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Insert<T>(T entry)
		{
			Insert(typeof (T), entry);
		}

		/// <summary>
		///     Insert a
		///     <paramref name="entry" />
		///     ,then selectes this entry based on the last inserted ID and creates a new model
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T InsertWithSelect<T>(T entry)
		{
			return InsertWithSelect(entry, Database, LoadCompleteResultBeforeMapping);
		}

		/// <summary>
		///     Creates AutoStatements in the size of RangerInsertPation
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void InsertRange<T>(IEnumerable<T> entry)
		{
			Database.RunInTransaction(c =>
			{
				var insertRangeCommand = CreateInsertRangeCommand(entry.ToArray(), c);
				//TODO 
				uint curr = 0;
				foreach (IDbCommand item in insertRangeCommand)
				{
					curr += RangerInsertPation;
					RaiseInsert(entry.Skip(((int) curr)).Take((int) RangerInsertPation), item, c);
					c.ExecuteNonQuery(item);
				}
			});
		}

		/// <summary>
		///     Creates the Multi Insert statement based on the Ranger property
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IDbCommand[] CreateInsertRangeCommand<T>(T[] entrys, IDatabase db)
		{
			var commands = new List<IDbCommand>();
			IDbCommand insertRange = null;

			uint toke = 0;
			var type = typeof (T);

			for (var i = 0; i < entrys.Count(); i++)
			{
				var singelCommand = _CreateInsert(type, entrys[i], db);

				if (insertRange == null)
				{
					insertRange = singelCommand;
					continue;
				}
				insertRange = db.MergeCommands(insertRange, singelCommand, true);
				toke++;

				if (toke == RangerInsertPation)
				{
					toke = 0;
					commands.Add(insertRange);
					insertRange = null;
				}
			}

			commands.Add(insertRange);
			return commands.ToArray();
		}

		/// <summary>
		///     Creates a single Insert Statement with the propertys of
		///     <paramref name="entry" />
		/// </summary>
		/// <returns></returns>
		public static IDbCommand _CreateInsert(Type type, object entry, IDatabase db)
		{
			var classInfo = type.GetClassInfo();

			var ignore =
				classInfo
					.PropertyInfoCaches
					.Select(s => s.Value)
					.Where(f => f.PrimaryKeyAttribute != null || f.InsertIgnore)
					.Select(s => s.DbName)
					.ToArray();

			var propertyInfos = type.FilterDbSchemaMapping(ignore).ToArray();
			var csvprops = type.CreatePropertyCsv(ignore);

			var values = "";
			for (var index = 0; index < propertyInfos.Length; index++)
				values = values + ("@" + index + ",");
			values = values.Remove(values.Length - 1);
			var query = "INSERT INTO " + type.GetTableName() + " ( " + csvprops + " ) VALUES ( " + values + " )";

			var orignialProps = type.GetPropertysViaRefection(ignore).ToArray();

			ValidateEntity(entry);
			return db.CreateCommandWithParameterValues(type, query, orignialProps, entry);
		}

		/// <summary>
		///     Creates a single insert statement for a
		///     <paramref name="entry" />
		///     uses
		///     <paramref name="parameter" />
		///     if possible
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateInsert(Type type, object entry, IDatabase db, params object[] parameter)
		{
			return type.CreateCommandOfClassAttribute<InsertFactoryMethodAttribute>(entry, db,
				(e, f) => _CreateInsert(type, e, f), parameter);
		}

		/// <summary>
		///     Creates and Executes a Insert statement for a given
		///     <paramref name="entry" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void Insert<T>(T entry, IDatabase db)
		{
			Insert(typeof (T), entry, db);
			//db.Run(s => { s.ExecuteNonQuery(CreateInsert(typeof(T), entry, s)); });
		}

		/// <summary>
		///     Creates and Executes a Insert statement for a given
		///     <paramref name="entry" />
		/// </summary>
		public static void Insert(Type type, object entry, IDatabase db)
		{
			db.Run(s => { s.ExecuteNonQuery(CreateInsert(type, entry, s)); });
		}

		/// <summary>
		///     Creates an insert command with appended LastInsertedIDCommand from the IDatabase interface
		/// </summary>
		/// <returns></returns>
		public static IDbCommand CreateInsertWithSelectCommand(Type type, object entry, IDatabase db)
		{
			var dbCommand = CreateInsert(type, entry, db);
			return db.MergeCommands(dbCommand, db.GetlastInsertedIdCommand());
		}

		/// <summary>
		///     Creates and Executes a Insert statement for a given
		///     <paramref name="type" />
		/// </summary>
		/// <returns></returns>
		public static object InsertWithSelect(Type type, object entry, IDatabase db, bool egarLoading)
		{
			return db.Run(s =>
			{
				var mergeCommands = CreateInsertWithSelectCommand(type, entry, db);
				RaiseInsert(entry, mergeCommands, s);
				return Select(type, s.GetSkalar(mergeCommands), s, egarLoading);
			});
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

		/// <summary>
		///     Creates and Executes a Insert statement for a given
		///     <paramref name="entry" />
		///     and selectes that
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T InsertWithSelect<T>(T entry, IDatabase db, bool egarLoading)
		{
			return (T) InsertWithSelect(typeof (T), entry, db, egarLoading);
		}
	}
}