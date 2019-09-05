#region

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.SqLite;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.Framework.SqLite
{
	public class SqLiteManager : IManagerImplementation
	{
		public const string SConnectionString = "Data Source={0};";
		private DbAccessLayer _expectWrapper;

		private string _dbFilePath;

		public DbAccessType DbAccessType
		{
			get { return DbAccessType.SqLite; }
		}

		public string ConnectionString
		{
			get { return SConnectionString; }
		}

		public DbAccessLayer GetWrapper(DbAccessType type, string testName)
		{
			if (_expectWrapper != null)
			{
				_expectWrapper.Database.CloseAllConnection();
			}

			SqLiteInteroptWrapper.EnsureSqLiteInteropt();
			//string dbname = "testDB";
			//var sqlLiteFileName = dbname + ".sqlite";

		    var sqLiteConnection = Environment.ExpandEnvironmentVariables(ConfigurationManager
		        .ConnectionStrings["RDBMS.SqLite.DefaultConnection"].ConnectionString);
		    var connectionRegex = new Regex("Data Source=(.*);").Match(sqLiteConnection);
		    
            _dbFilePath = Path.Combine(connectionRegex.Groups[1].Value, string.Format("YAORM_SqLite_{0}.db", testName));
		    var connection = sqLiteConnection.Replace(connectionRegex.Groups[1].Value, _dbFilePath);

            if (File.Exists(_dbFilePath))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				File.Delete(_dbFilePath);
			}

			File.Create(_dbFilePath).Dispose();

			//var file = MemoryMappedFile.CreateNew(dbname, 10000, MemoryMappedFileAccess.ReadWrite);

			//tempPath = Path.GetTempFileName() + dbname + "sqLite";

			_expectWrapper = new DbAccessLayer(new DataAccess.SqLite.SqLiteStrategy(connection), new DbConfig(true));
			foreach (var databaseMeta in MetaManager.DatabaseMetas)
			{
				_expectWrapper.ExecuteGenericCommand(_expectWrapper.Database.CreateCommand(databaseMeta.Value.CreationCommand(DbAccessType)));
			}
			return _expectWrapper;
		}

		public void FlushErrorData()
		{
			Console.WriteLine($"Error Data for '{_dbFilePath}'");
		}

		public void Clear()
		{
			var connectionCounter = SqLiteStrategy.ConnectionCounter.Where(f => f.Key == _expectWrapper.Database.DatabaseFile)
			                              .SelectMany(e => e.Value.Connections)
			                              .Select(e =>
			                              {
				                              IDbConnection conn;
				                              e.Key.TryGetTarget(out conn);
				                              return conn;
			                              })
			                              .Where(f => f != null)
			                              .ToArray();

			if (connectionCounter.Any())
			{
				Console.WriteLine("Clear detected Undisposed Connections");
				foreach (var sqLiteConnectionCounter in connectionCounter)
				{
					Console.WriteLine("\t - " + sqLiteConnectionCounter.State);
				}
			}

			_expectWrapper.Database.CloseAllConnection();

			if (File.Exists(_dbFilePath))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				try
				{
					File.Delete(_dbFilePath);
				}
				catch (Exception)
				{
					Console.WriteLine($"Could not cleanup the SqLite File '{_dbFilePath}'");
				}
			}
		}
	}

}