using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests
{
	public class MsSqlManager : IManager
	{
		public MsSqlManager()
		{
			ConnectionType = "DefaultConnection";
			if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
			{
				ConnectionType = "CiConnection";
			}
		}

		private static DbAccessLayer expectWrapper;

		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
		}

		private static string _connectionString;
		public string ConnectionType { get; set; }

		public string ConnectionString
		{
			get
			{
				if (_connectionString != null)
					return _connectionString;
				_connectionString = ConfigurationManager.ConnectionStrings[ConnectionType].ConnectionString;
				Console.WriteLine("-------------------------------------------");
				Console.WriteLine("Connection String");
				Console.WriteLine(_connectionString);
				return _connectionString;
			}
		}

		public DbAccessLayer GetWrapper()
		{
			const string dbname = "testDB";
			if (expectWrapper != null)
			{
				expectWrapper.Database.CloseAllConnection();
			}

			var redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				dbname);

			expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString);
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE ", dbname)));

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(redesginDatabase));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", dbname)));

			expectWrapper = new DbAccessLayer(DbAccessType, string.Format(ConnectionString + "Initial Catalog={0};", dbname));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(UsersMeta.CreateMsSql));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(BookMeta.CreateMsSQl));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(ImageMeta.CreateMsSQl));

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
}