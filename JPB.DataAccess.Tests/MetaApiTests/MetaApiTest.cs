using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Tests.TestModels.MetaAPI;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.MetaApiTests
#if MSSQL
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class MetaApiTest
	{
		[Test]
		public void ClassCreating()
		{
			var cache = new DbConfig();
			cache.Include<ClassCreating>();
			dynamic result = cache.SClassInfoCaches.First().DefaultFactory();
			Assert.IsNotNull(result);
		}

		[Test]
		public void TestStructCreating()
		{
			var cache = new DbConfig();
			cache.Include<StructCreating>();
			dynamic result = cache.SClassInfoCaches.First().DefaultFactory();
			Assert.IsNotNull(result);
		}
	}
}