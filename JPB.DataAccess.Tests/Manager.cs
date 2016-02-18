using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#if SqLite
using System.IO;
#endif

namespace JPB.DataAccess.Tests
{
	public class Manager
	{
		private static DbAccessLayer expectWrapper;

#if MsSql
		public const string SConnectionString = "Data Source=(localdb)\\ProjectsV12;Integrated Security=True;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
		}

				public string ConnectionString
		{
			get { return SConnectionString; }
		}
#endif

#if SqLite
		public const string SConnectionString = "Data Source=testDB.sqlite;Version=3;New=True;PRAGMA journal_mode=WAL;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.SqLite; }
		}

				public string ConnectionString
		{
			get { return SConnectionString; }
		}
#endif

		public DbAccessLayer GetWrapper()
		{
			if (expectWrapper != null)
				return expectWrapper;

			string dbname = "testDB";

#if SqLite
			var sqlLiteFileName = dbname + ".sqlite";
			if (File.Exists(sqlLiteFileName))
				File.Delete(sqlLiteFileName);
			File.Create(sqlLiteFileName).Close();
			expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString);
			expectWrapper.ExecuteGenericCommand(
			expectWrapper.Database.CreateCommand(
				string.Format(
					"CREATE TABLE {0}({1} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, {2} TEXT);",
					UsersMeta.UserTable, UsersMeta.UserIDCol, UsersMeta.UserNameCol)));
#endif

#if MsSql
			var redesginDatabase = string.Format(
	"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
	dbname);

			expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString);

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(redesginDatabase));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", dbname)));

			expectWrapper = new DbAccessLayer(DbAccessType, string.Format(ConnectionString + "Initial Catalog={0};", dbname));
			expectWrapper.ExecuteGenericCommand(
			expectWrapper.Database.CreateCommand(
				string.Format(
					"CREATE TABLE {0} ( {1} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL, {2} NVARCHAR(MAX));",
					UsersMeta.UserTable, UsersMeta.UserIDCol, UsersMeta.UserNameCol)));

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
																					 "AS BEGIN " +
																					 "SELECT * FROM Users " +
																					 "END"));

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
																		  "AS BEGIN " +
																		  "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
																		  "END "));
#endif
			DbConfig.ConstructorSettings.CreateDebugCode = false;

			Assert.NotNull(expectWrapper);
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
	}
}