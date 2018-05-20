#region

using System;
using System.Configuration;
using System.Text;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

#endregion

namespace JPB.DataAccess.Tests
{
	public class MsSqlManager : IManagerImplementation
	{
		private readonly StringBuilder _errorText = new StringBuilder();

		private string _connectionString;
		private DbAccessLayer _expectWrapper;

		public MsSqlManager()
		{
			ConnectionType = "DefaultConnection";
			if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
			{
				ConnectionType = "CiConnection";
			}
		}

		public string ConnectionType { get; set; }

		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
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

		private string _dbname;

		public DbAccessLayer GetWrapper(DbAccessType type, string testName)
		{
			_dbname = string.Format("YAORM_2_TestDb_Test_MsSQL_{0}", testName);
			if (_expectWrapper != null)
			{
				_expectWrapper.Database.CloseAllConnection();
			}
			_expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString, new DbConfig(true));

			var redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				_dbname);

			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(redesginDatabase));
			_expectWrapper.ExecuteGenericCommand(
				_expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", _dbname)));

			_expectWrapper = new DbAccessLayer(DbAccessType,
				string.Format(ConnectionString + "Initial Catalog={0};", _dbname), new DbConfig(true));
			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(UsersMeta.CreateMsSql));
			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(BookMeta.CreateMsSQl));
			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(ImageMeta.CreateMsSQl));

			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
			                                                                         "AS BEGIN " +
			                                                                         "SELECT * FROM Users " +
			                                                                         "END"));

			_expectWrapper.ExecuteGenericCommand(
				_expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
				                                     "AS BEGIN " +
				                                     "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
				                                     "END "));


			return _expectWrapper;
		}

		public void FlushErrorData()
		{
			Console.WriteLine(_errorText.ToString());
			_errorText.Clear();
		}

		public void Clear()
		{
			if (_expectWrapper != null)
			{
				_expectWrapper.Database.CloseAllConnection();
				
				var redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
				_dbname);

				var masterWrapper = new DbAccessLayer(DbAccessType,
				string.Format(ConnectionString + "Initial Catalog=master;"), new DbConfig(true));

				masterWrapper.ExecuteGenericCommand(masterWrapper.Database.CreateCommand(redesginDatabase));

				redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				_dbname);

				masterWrapper.ExecuteGenericCommand(masterWrapper.Database.CreateCommand(redesginDatabase));

				_expectWrapper = null;
			}
		}
	}
}