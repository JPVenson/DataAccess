using System.IO;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests
{
    public class SqLiteManager : IManager
    {
        public const string SConnectionString = "Data Source={0};";
        private static DbAccessLayer expectWrapper;

        private string tempPath;

        public DbAccessType DbAccessType
        {
            get { return DbAccessType.SqLite; }
        }

        public string ConnectionString
        {
            get { return SConnectionString; }
        }

        public DbAccessLayer GetWrapper(DbAccessType type)
        {
            if (expectWrapper != null)
                expectWrapper.Database.CloseAllConnection();

            //string dbname = "testDB";
            //var sqlLiteFileName = dbname + ".sqlite";

            tempPath = Path.GetTempFileName();

            expectWrapper = new DbAccessLayer(DbAccessType, string.Format(ConnectionString, tempPath));
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