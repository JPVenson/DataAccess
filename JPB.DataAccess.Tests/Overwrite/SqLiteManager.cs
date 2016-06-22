using System.IO;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests
{
	public class SqLiteManager : IManager
	{
		private static DbAccessLayer expectWrapper;

		public const string SConnectionString = "Data Source={0};";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.SqLite; }
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
			expectWrapper = new DbAccessLayer(DbAccessType, string.Format(ConnectionString, sqlLiteFileName));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(UsersMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(BookMeta.CreateSqLite));
			expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(ImageMeta.CreateSqLite));
			return expectWrapper;
		}
	}
}