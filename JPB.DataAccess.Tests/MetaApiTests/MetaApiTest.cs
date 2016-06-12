using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
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

		[Test]
		public void FakeCreation()
		{
			var cache = new DbConfig(true);
			Assert.That(() =>
			{
				var fakeType = cache.GetFake("FakeType");
				Assert.That(fakeType.Name, Is.Not.Null.And.EqualTo("FakeType"));
				Assert.That(fakeType.Propertys.Count, Is.Empty);
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(6));
				Assert.That(fakeType.Attributes.Count, Is.Empty);
			}, Throws.Nothing);
		}

		[Test]
		public void FakeStructProperty()
		{
			var cache = new DbConfig(true);
			Assert.That(() =>
			{
				var fakeType = cache.GetFake("FakeType");
				fakeType.Propertys.Add("Test", new DbAutoPropertyInfoCache<long>("Test"));

				Assert.That(fakeType.Propertys.Count, Is.EqualTo(1));
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(6));
				Assert.That(fakeType.Attributes, Is.Empty);

				var refElement = fakeType.DefaultFactory();
				var originalValue = 12;

				Assert.That(refElement, Is.Not.Null.And.TypeOf(fakeType.Type));

				var propTest = fakeType.Propertys["Test"];
				Assert.That(propTest, Is.Not.Null);
				propTest.Setter.Invoke(refElement, originalValue);
				var propValue = propTest.Getter.Invoke(refElement);
				Assert.That(propValue, Is.Not.Null.And.EqualTo(originalValue));
			}, Throws.Nothing);
		}
	}
}