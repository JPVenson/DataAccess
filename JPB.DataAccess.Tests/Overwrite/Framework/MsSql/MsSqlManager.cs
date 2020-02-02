#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.AdoWrapper.Remoting;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MySql;
using JPB.DataAccess.SqLite;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.Framework.MsSql
{
	public class RemotingManager : IManagerImplementation
	{
		private readonly IManagerImplementation _subManager;

		public RemotingManager(IManagerImplementation subManager)
		{
			_subManager = subManager;
		}

		public DbAccessType DbAccessType
		{
			get { return _subManager.DbAccessType | DbAccessType.Remoting; }
		}

		public string ConnectionString
		{
			get { return _subManager.ConnectionString; }
		}

		public class LocalRemotingStrategy : RemotingStrategyExternal
		{
			private readonly RemotingManager _manager;
			public RemotingConsumerServer RemotingConsumerServer { get; set; }

			public LocalRemotingStrategy(DbAccessType emulateDbType, DbConfig config, RemotingManager manager,
				string instanceName) : base(emulateDbType, config)
			{
				_manager = manager;
				var wrapper = _manager._subManager.GetWrapper(_manager._subManager.DbAccessType, instanceName);
				RemotingConsumerServer = new RemotingConsumerServer(() =>
					{
						return wrapper;
					});
			}

			public override object Clone()
			{
				throw new NotImplementedException();
			}

			public override IDataPager<T> CreatePager<T>()
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.Experimental:
						break;
					case DbAccessType.Unknown:
						break;
					case DbAccessType.MsSql:
						return new MsSqlDataPager<T>();
					case DbAccessType.MySql:
						return new MySqlDataPager<T>();
					case DbAccessType.OleDb:
						break;
					case DbAccessType.Obdc:
						break;
					case DbAccessType.SqLite:
						return new SqLiteDataPager<T>();
					case DbAccessType.Remoting:
						break;
				}
				throw new ArgumentOutOfRangeException();
			}

			public override IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.MsSql:
						return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] ON", classInfoTableName), conn);
					case DbAccessType.MySql:
						return null;
					case DbAccessType.SqLite:
						return null;
				}
				throw new NotImplementedException();
			}

			public override IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.MsSql:
						return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] OFF", classInfoTableName), conn);
					case DbAccessType.MySql:
						return null;
					case DbAccessType.SqLite:
						return null;
				}
				throw new NotImplementedException();
			}

			public override string RegisterConnection()
			{
				return RemotingConsumerServer.RegisterConnection();
			}

			public override void CloseConnection(string connectionId)
			{
				RemotingConsumerServer.CloseConnection(connectionId);
			}

			public override string RegisterTransaction(string connectionId)
			{
				return RemotingConsumerServer.RegisterTransaction(connectionId);
			}

			public override bool RollbackTransaction(string connectionId, string transactionId)
			{
				return RemotingConsumerServer.RollbackTransaction(connectionId, transactionId);
			}

			public override bool CommitTransaction(string connectionId, string transactionId)
			{
				return RemotingConsumerServer.CommitTransaction(connectionId, transactionId);
			}

			public override int ExecuteQuery(string commandGraph, string connectionId, string transactionId)
			{
				return RemotingConsumerServer.ExecuteQuery(commandGraph, connectionId, transactionId);
			}

			public override object ExecuteScalar(string commandGraph, string connectionId, string transactionId)
			{
				return RemotingConsumerServer.ExecuteScalar(commandGraph, connectionId, transactionId);
			}

			public override IEnumerable<IEnumerable<IDataRecord>> ExecuteCommand(string commandGraph, string connectionId, string transactionId, out int recordsAffected)
			{
				var enumerateCommand = RemotingConsumerServer.EnumerateCommand(commandGraph, connectionId, transactionId, out recordsAffected);
				Assert.That(enumerateCommand, Is.XmlSerializable.Or.BinarySerializable);
				return enumerateCommand;
			}
		}

		private LocalRemotingStrategy _strategy;

		public DbAccessLayer GetWrapper(DbAccessType type, string instanceName)
		{
			return new DbAccessLayer(_strategy = new LocalRemotingStrategy(type, new DbConfig(), this, instanceName + "RMT_"));
		}

		public void FlushErrorData()
		{
			_subManager.FlushErrorData();
		}

		public void Clear()
		{
			if (_strategy?.RemotingConsumerServer.HasOpenConnection() == true)
			{
				Assert.Fail("There are still open Connections");
			}
			_subManager.Clear();
		}
	}

	public class MsSqlManager : IManagerImplementation
	{
		private readonly StringBuilder _errorText = new StringBuilder();

		private string _connectionString;
		private DbAccessLayer _expectWrapper;

		public MsSqlManager()
		{
			ConnectionType = "RDBMS.MsSql.DefaultConnection";
			if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
			{
				ConnectionType = "RDBMS.MsSql.CiConnection";
			}
			if (Environment.GetEnvironmentVariable("build.with") == "VSTS")
			{
				ConnectionType = "RDBMS.MsSql.VSTSConnection";
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

			var dbConfig = new DbConfig(true);
			dbConfig.EnableInstanceThreadSafety = true;
			_expectWrapper = new DbAccessLayer(DbAccessType, ConnectionString, dbConfig);

			var redesginDatabase = string.Format(
				"IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				_dbname);

			_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(redesginDatabase));
			_expectWrapper.ExecuteGenericCommand(
				_expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", _dbname)));

			_expectWrapper = new DbAccessLayer(DbAccessType,
				string.Format(ConnectionString + "Initial Catalog={0};", _dbname), dbConfig);

			foreach (var databaseMeta in MetaManager.DatabaseMetas)
			{
				_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(databaseMeta.Value.CreationCommand(DbAccessType)));
			}

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