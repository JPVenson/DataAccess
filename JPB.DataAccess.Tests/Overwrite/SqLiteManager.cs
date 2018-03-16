#region

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

#endregion

namespace JPB.DataAccess.Tests
{
	public class SqLiteManager : IManagerImplementation
	{
		public const string SConnectionString = "Data Source={0};";
		private DbAccessLayer expectWrapper;

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
			if (expectWrapper != null)
			{
				expectWrapper.Database.CloseAllConnection();
			}
			//load SqLite in domain
			var type1 = typeof(SqLite.SqLite);
			//string dbname = "testDB";
			//var sqlLiteFileName = dbname + ".sqlite";
			_dbFilePath = string.Format("YAORM_SqLite_{0}.db", testName);
			if (File.Exists(_dbFilePath))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				File.Delete(_dbFilePath);
			}
			File.Create(_dbFilePath).Dispose();
			var connection = string.Format(ConnectionString, _dbFilePath);

			//var file = MemoryMappedFile.CreateNew(dbname, 10000, MemoryMappedFileAccess.ReadWrite);

			//tempPath = Path.GetTempFileName() + dbname + "sqLite";

			expectWrapper = new DbAccessLayer(new SqLite.SqLite(connection), new DbConfig(true));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(UsersMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(BookMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(ImageMeta.CreateSqLite));
			var userses = expectWrapper.Select<Users>();
			return expectWrapper;
		}

		public void FlushErrorData()
		{
		}

		public void Clear()
		{
			expectWrapper.Database.CloseAllConnection();

			if (File.Exists(_dbFilePath))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				try
				{
					File.Delete(_dbFilePath);
				}
				catch (Exception e)
				{
					Console.WriteLine("Could not cleanup the SqLite File");
				}
			}
		}
	}
}