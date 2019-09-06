#region

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Tests.Base.TestModels.MetaAPI;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.MetaApiTests

{
	[TestFixture]
	public class MetaApiTest
	{
		[Test]
		public void DictionaryClassCreating()
		{
			var cache = new DbConfig(true);

			var dyn = new Dictionary<string, object>();
			dyn.Add("", "");

			var orCreateClassInfoCache = cache.GetOrCreateClassInfoCache(dyn.GetType());
			Assert.That(orCreateClassInfoCache, Is.Not.Null);
			Assert.That(orCreateClassInfoCache.Propertys, Is.Not.Null);
			Assert.That(orCreateClassInfoCache.Propertys, Contains.Key("Keys"));
			Assert.That(orCreateClassInfoCache.Propertys, Contains.Key("Values"));
			Assert.That(orCreateClassInfoCache.Propertys, !Contains.Key("Item"));
			var nTestDic = orCreateClassInfoCache.DefaultFactory();
			Assert.That(nTestDic, Is.Not.Null);
			Assert.That(nTestDic, Is.TypeOf(typeof(Dictionary<string, object>)));
			Assert.That(nTestDic, Is.Empty);
		}

		[Test]
		public void DynamicClassCreating()
		{
			var cache = new DbConfig(true);

			var dyn = new
			{
				TestProp = ""
			};

			var orCreateClassInfoCache = cache.GetOrCreateClassInfoCache(dyn.GetType());
			Assert.That(orCreateClassInfoCache, Is.Not.Null);
			Assert.That(orCreateClassInfoCache.Propertys, Is.Not.Null);
			Assert.That(orCreateClassInfoCache.Propertys, Contains.Key("TestProp"));
			Assert.That(() => orCreateClassInfoCache.DefaultFactory(), Throws.Exception);
		}

		[Test]
		public void ClassCreating()
		{
			var cache = new DbConfig(true);
			cache.Include<ClassCreating>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null.And.TypeOf<ClassCreating>());
		}

		[Test]
		public void ClassCreatingWithArguments()
		{
			var cache = new DbConfig(true);
			cache.Include<ClassCreatingWithArguments>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Throws.Exception);
		}

		[Test]
		public void ParallelAccessGlobal()
		{
			Parallel.For(0, 500, nr =>
			{
				var cache = new DbConfig(false);
				cache.EnableInstanceThreadSafety = true;
				Assert.That(cache, Is.Not.Null);
				Assert.That(cache.IsGlobal, Is.True);
				Assert.That(cache.EnableInstanceThreadSafety, Is.True);
				Assert.That(() => { cache.Include<ClassCreating>(); }, Throws.Nothing);
				Assert.That(cache.SClassInfoCaches.Count, Is.EqualTo(1));
				Assert.That(() => { Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null); },
					Throws.Nothing);
			});
			DbConfig.Clear();
		}

		[Test]
		public void ParallelAccessLocal()
		{
			Parallel.For(0, 500, nr =>
			{
				var cache = new DbConfig(true);
				Assert.That(cache, Is.Not.Null);
				Assert.That(cache.IsGlobal, Is.False);
				Assert.That(cache.EnableInstanceThreadSafety, Is.False);
				Assert.That(cache.SClassInfoCaches, Is.Empty);
				Assert.That(() => { cache.Include<ClassCreating>(); }, Throws.Nothing);
				Assert.That(cache.SClassInfoCaches.Count, Is.EqualTo(1));
				Assert.That(() => { Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null); },
					Throws.Nothing);
			});
		}

		[Test]
		public void TestStructCreating()
		{
			var cache = new DbConfig(true);
			cache.Include<StructCreating>();
			Assert.That(() => cache.SClassInfoCaches.First().DefaultFactory(), Is.Not.Null.And.TypeOf<StructCreating>());
		}

		[Test]
		public void TestFrameworkDirectType()
		{
			var cache = new DbConfig(true);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(string)).IsMsCoreFrameworkType, Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(int)).IsMsCoreFrameworkType, Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(byte)).IsMsCoreFrameworkType, Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(decimal)).IsMsCoreFrameworkType, Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(decimal?)).IsMsCoreFrameworkType, Is.True);
		}


		[Test]
		public void TestFrameworkType()
		{
			var cache = new DbConfig(true);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(ICollection<>)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(ICollection)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(IDataAdapter)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(IDataRecord)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(XmlReadMode)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(XmlReadMode?)).IsFrameworkType(), Is.True);
			Assert.That(cache.GetOrCreateClassInfoCache(typeof(XmlAnyAttributeAttribute)).IsFrameworkType(), Is.True);
		}
	}
}