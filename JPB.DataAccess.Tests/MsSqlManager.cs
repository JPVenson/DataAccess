using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests
{
	public class MsSqlManager : IManager
	{
		public MsSqlManager()
		{
			ConnectionType = "DefaultConnection";

			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("Environment Variables");
			Hashtable variables = Environment.GetEnvironmentVariables() as Hashtable;
			foreach (DictionaryEntry item in variables)
			{
				Console.WriteLine(string.Format("{0} - '{1}'", item.Key, item.Value));
			}

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
}