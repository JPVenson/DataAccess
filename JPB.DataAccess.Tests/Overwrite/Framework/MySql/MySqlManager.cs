#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.Framework.MySql
{
	public class MySqlManager : IManagerImplementation
	{
		public static HashSet<string> CurrentUsedDatabases { get; set; } = new HashSet<string>();

		private DbAccessLayer _expectWrapper;
		private string _connectionString;
		private readonly StringBuilder _errorText = new StringBuilder();

		public MySqlManager()
		{
			ConnectionType = "RDBMS.MySql.DefaultConnection";
			if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
			{
				ConnectionType = "CiConnectionMySQL";
			}
		}

		public string ConnectionType { get; set; }

		public string DatabaseName { get; set; }

		public MySqlConnector.LoggerDelegate Logger { get; set; }

		public DbAccessLayer GetWrapper(DbAccessType type, string testName)
		{
			if (ConfigurationManager.AppSettings["RDBMS.MySql.StartManagedServer"] == "True")
			{
				MySqlConnectorInstance.Instance.CreateAndRunIfNot();
			}

			Logger = MySqlConnectorInstance.Instance.AttachLogger();
			DatabaseName = $"YAORM_TestDb_{testName}";
			using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider())
			{
				md5CryptoServiceProvider.Initialize();
				DatabaseName = "YAORM_TestDb_" + md5CryptoServiceProvider.ComputeHash(Encoding.Default.GetBytes(DatabaseName)).Select(e => e.ToString("X2")).Aggregate((e,f) => e + f);
			}

			if (_expectWrapper != null)
			{
				_expectWrapper.Database.CloseAllConnection();
			}

			//Darn MySql with only 64 chars?
			if (DatabaseName.Length > 64)
			{
				DatabaseName = DatabaseName.Replace("_", "");
			}	
			
			if (DatabaseName.Length > 64)
			{
				DatabaseName = DatabaseName.Replace("YAORM_TestDb_", "");
			}	
			
			if (DatabaseName.Length > 64)
			{
				DatabaseName = DatabaseName.Substring(DatabaseName.Length - 64);
			}

			lock (CurrentUsedDatabases)
			{
				if (CurrentUsedDatabases.Contains(DatabaseName))
				{
					throw new InvalidOperationException($"The input types has been duplicated {DatabaseName} ");
				}

				CurrentUsedDatabases.Add(DatabaseName);
			}

			var redesginDatabase = string.Format(
				"DROP DATABASE IF EXISTS {0};",
				DatabaseName);

			_expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString, new DbConfig(true));
			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(redesginDatabase));
			_expectWrapper.ExecuteGenericCommand(
				_expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0};", DatabaseName)));
			_expectWrapper = new DbAccessLayer(DbAccessType, string.Format(ConnectionString + "Database={0};", DatabaseName));
			foreach (var databaseMeta in MetaManager.DatabaseMetas)
			{
				_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(databaseMeta.Value.CreationCommand(DbAccessType)));
			}
			//_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
			//																		 "AS BEGIN " +
			//																		 "SELECT * FROM Users " +
			//																		 "END"));
			//_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
			//																		 "AS BEGIN " +
			//																		 "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
			//																		 "END "));
			return _expectWrapper;
		}

		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MySql; }
		}

		public string ConnectionString
		{
			get
			{
				if (_connectionString != null)
				{
					return _connectionString;
				}
				_connectionString = ConfigurationManager.ConnectionStrings[ConnectionType].ConnectionString;
				_errorText.AppendLine("-------------------------------------------");
				_errorText.AppendLine("Connection String");
				_errorText.AppendLine(_connectionString);
				return _connectionString;
			}
		}

		public void FlushErrorData()
		{
			_errorText.AppendLine("-------------------------------------------");
			TestContext.Error.WriteLine(_errorText.ToString());
			foreach (var loggerLogLine in Logger.LogLines)
			{
				TestContext.Error.WriteLine(loggerLogLine.Orginal);
			}
			_errorText.Clear();
		}

		public void Clear()
		{
			if (_expectWrapper != null)
			{
				var wrapper = _expectWrapper;
				lock (MySqlConnectorInstance.Instance)
				{
					wrapper.Database.CloseAllConnection();
				}
			}

			var dbAccessLayer = new DbAccessLayer(DbAccessType, string.Format(ConnectionString));
			dbAccessLayer.ExecuteGenericCommand("DROP DATABASE IF EXISTS " + DatabaseName);
		}
	}
}