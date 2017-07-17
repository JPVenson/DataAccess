#region

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
		public const string SConnectionString = "Data Source=:memory:;";
		private DbAccessLayer expectWrapper;

		private string tempPath;

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
				expectWrapper.Database.CloseAllConnection();

			//string dbname = "testDB";
			//var sqlLiteFileName = dbname + ".sqlite";
			var dbname = string.Format("YAORM_SqLite_{0}", testName);
			tempPath = string.Format(ConnectionString, dbname);

			//var file = MemoryMappedFile.CreateNew(dbname, 10000, MemoryMappedFileAccess.ReadWrite);

			//tempPath = Path.GetTempFileName() + dbname + "sqLite";

			expectWrapper = new DbAccessLayer(DbAccessType, tempPath, new DbConfig(true));
			expectWrapper.Database.Connect();
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(UsersMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(BookMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(ImageMeta.CreateSqLite));
			return expectWrapper;
		}

		public void FlushErrorData()
		{
		}

		public void Clear()
		{
			expectWrapper.Database.CloseAllConnection();

			if (File.Exists(tempPath))
				File.Delete(tempPath);
		}
	}
}