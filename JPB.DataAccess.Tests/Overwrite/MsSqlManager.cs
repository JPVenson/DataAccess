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
		private DbAccessLayer expectWrapper;

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

		public DbAccessLayer GetWrapper(DbAccessType type, string testName)
		{
			var dbname = string.Format("YAORM_2_TestDb_Test_MsSQL_{0}", testName);
			if (expectWrapper != null)
			{
				expectWrapper.Database.CloseAllConnection();
			}

			expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString, new DbConfig(true));
			//try
			//{
			//	expectWrapper.ExecuteGenericCommand(
			//		expectWrapper.Database.CreateCommand(
			//			string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE ", dbname)));
			//}
			//catch (Exception)
			//{
			//	_errorText.AppendLine("Db does not exist");
			//}
			var redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				dbname);

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(redesginDatabase));
			expectWrapper.ExecuteGenericCommand(
				expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", dbname)));

			expectWrapper = new DbAccessLayer(DbAccessType,
				string.Format(ConnectionString + "Initial Catalog={0};", dbname), new DbConfig(true));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(UsersMeta.CreateMsSql));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(BookMeta.CreateMsSQl));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(ImageMeta.CreateMsSQl));

			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
			                                                                         "AS BEGIN " +
			                                                                         "SELECT * FROM Users " +
			                                                                         "END"));

			expectWrapper.ExecuteGenericCommand(
				expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
				                                     "AS BEGIN " +
				                                     "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
				                                     "END "));


			return expectWrapper;
		}

		public void FlushErrorData()
		{
			Console.WriteLine(_errorText.ToString());
			_errorText.Clear();
		}

		public void Clear()
		{
			if (expectWrapper != null)
			{
				expectWrapper.Database.CloseAllConnection();
			}
		}
	}
}