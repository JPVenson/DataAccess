using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Tests.TestModels.MetaAPI;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.MetaApiTests
#if MsSql
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
			var cache = new DbConfig(true);
			cache.Include<ClassCreating>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null);
		}

		[Test]
		public void TestStructCreating()
		{
			var cache = new DbConfig(true);
			cache.Include<StructCreating>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null);
		}

		[Test]
		public void ClassCreatingWithArguments()
		{
			var cache = new DbConfig(true);
			cache.Include<ClassCreatingWithArguments>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Throws.Exception);
		}
	}
}