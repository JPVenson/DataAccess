using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.UnitTests.TestModels.MetaAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.UnitTests
{
	[TestClass]
	public class MetaApiTest
	{
		public MetaApiTest()
		{

		}

		[TestMethod]
		public void TestStructCreating()
		{
			var cache = new DbConfig();
			cache.Include<StructCreating>();
			var result = cache.SClassInfoCaches.First().DefaultFactory();
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ClassCreating()
		{
			var cache = new DbConfig();
			cache.Include<ClassCreating>();
			var result = cache.SClassInfoCaches.First().DefaultFactory();
			Assert.IsNotNull(result);
		}
	}
}
