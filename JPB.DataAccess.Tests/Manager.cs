using System.Reflection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
using NUnit.Framework;

#if SqLite
using System.IO;
#endif

namespace JPB.DataAccess.Tests
{
	public interface IManager
	{
		DbAccessLayer GetWrapper();
	}

	public class Manager : IManager
	{
		public DbAccessType GetElementType()
		{
			var customAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<CategoryAttribute>();
			if (customAttribute.Name == "MsSQL")
			{
				return DbAccessType.MsSql;
			}
			else if(customAttribute.Name == "SqLite")
			{
				return DbAccessType.SqLite;
			}
			return DbAccessType.Unknown;

			//var category = TestContext.CurrentContext.Test.Properties["Category"];

			//if (category.Contains("MsSQL"))
			//{
			//	return DbAccessType.MsSql;
			//}
			//else if (category.Contains("SqLite"))
			//{
			//	return DbAccessType.SqLite;
			//}
			//return DbAccessType.Unknown;
		}

		public DbAccessLayer GetWrapper()
		{
			DbAccessLayer expectWrapper = null;

			if (GetElementType() == DbAccessType.MsSql)
			{
				expectWrapper = new MsSqlManager().GetWrapper();
			}
			else if (GetElementType() == DbAccessType.SqLite)
			{
				expectWrapper = new SqLiteManager().GetWrapper();
			}

			Assert.NotNull(expectWrapper, "This test cannot run as no Database Variable is defined");
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			DbConfig.ConstructorSettings.CreateDebugCode = false;
			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
	}
}


/*

#if MsSql
		public const string SConnectionString = "Data Source=(localdb)\\ProjectsV12;Integrated Security=True;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.MsSql; }
		}

				public string ConnectionString
		{
			get { return SConnectionString; }
		}
#endif

#if SqLite
		public const string SConnectionString = "Data Source=testDB.sqlite;Version=3;New=True;PRAGMA journal_mode=WAL;";
		public DbAccessType DbAccessType
		{
			get { return DbAccessType.SqLite; }
		}

				public string ConnectionString
		{
			get { return SConnectionString; }
		}
#endif

		public DbAccessLayer GetWrapper()
		{
			if (expectWrapper != null)
				return expectWrapper;

			string dbname = "testDB";

#if SqLite
			
#endif

#if MsSql
		
#endif
			DbConfig.ConstructorSettings.CreateDebugCode = false;

			Assert.NotNull(expectWrapper);
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
*/
