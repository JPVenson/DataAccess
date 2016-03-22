using System;
using System.IO;
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
	public interface IManager
	{
		DbAccessLayer GetWrapper();
	}

	public class SqLiteManager : IManager
	{
		private DbAccessLayer expectWrapper;

		public const string SConnectionString = "Data Source=(localdb)\\ProjectsV12;Integrated Security=True;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
		}

		public string ConnectionString
		{
			get { return SConnectionString; }
		}

		public DbAccessLayer GetWrapper()
		{
			if (expectWrapper != null)
				return expectWrapper;

			string dbname = "testDB";



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
		
			return expectWrapper;
		}
	}

	public class MsSQLManager : IManager
	{
		private static DbAccessLayer expectWrapper;

		public const string SConnectionString = "Data Source=(localdb)\\ProjectsV12;Integrated Security=True;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
		}

		public string ConnectionString
		{
			get { return SConnectionString; }
		}

		public DbAccessLayer GetWrapper()
		{
			if (expectWrapper != null)
				return expectWrapper;

			string dbname = "testDB";

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

			return expectWrapper;
		}
	}

	public class Manager : IManager
	{
		public DbAccessType GetElementType()
		{
			var category = TestContext.CurrentContext.Test.Properties["Category"];

			if (category.Contains("MsSQL"))
			{
				return DbAccessType.MsSql;
			}
			else if (category.Contains("SqLite"))
			{
				return DbAccessType.SqLite;
			}
			return DbAccessType.Unknown;
		}

		public DbAccessLayer GetWrapper()
		{
			DbAccessLayer expectWrapper = null;

			if (GetElementType() == DbAccessType.MsSql)
			{
				expectWrapper = new MsSQLManager().GetWrapper();
			}
			else if (GetElementType() == DbAccessType.MsSql)
			{
				expectWrapper = new SqLiteManager().GetWrapper();
			}

			Assert.NotNull(expectWrapper, "This test cannot run as no Database Variable is defined");
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			DbConfig.ConstructorSettings.CreateDebugCode = false;
			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
	}
}


/*

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
			
#endif

#if MsSql
		
#endif
			DbConfig.ConstructorSettings.CreateDebugCode = false;

			Assert.NotNull(expectWrapper);
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
*/
