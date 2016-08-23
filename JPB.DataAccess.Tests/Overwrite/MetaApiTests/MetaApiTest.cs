using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Tests.Base;
using JPB.DataAccess.Tests.Base.TestModels.MetaAPI;
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
				Assert.That(fakeType.Propertys, Is.Empty);
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(6));
				Assert.That(fakeType.Attributes, Is.Empty);
			}, Throws.Nothing);
		}

		[Test]
		public void FakeClassStructProperty()
		{
			var cache = new DbConfig(true);
			Assert.That(() =>
			{
				var fakeType = cache.GetFake("FakeType");

				fakeType.Propertys.Add("Test", new DbAutoStaticPropertyInfoCache<long>("Test", fakeType.Type));

				Assert.That(fakeType.Propertys.Count, Is.EqualTo(1));
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(6));
				Assert.That(fakeType.Attributes, Is.Empty);

				var refElement = fakeType.DefaultFactory();
				var originalValue = 12L;

				Assert.That(refElement, Is.Not.Null.And.TypeOf(fakeType.Type));

				var propTest = fakeType.Propertys["Test"];
				Assert.That(propTest, Is.Not.Null);
				propTest.Setter.Invoke(refElement, originalValue);
				var propValue = propTest.Getter.Invoke(refElement);
				Assert.That(propValue, Is.Not.Null.And.EqualTo(originalValue));
			}, Throws.Nothing);
		}

		[Test]
		public void FakeClassComplexProperty()
		{
			var cache = new DbConfig(true);
			Assert.That(() =>
			{
				var fakeType = cache.GetFake("FakeType");

				fakeType.Propertys.Add("Test", new DbAutoStaticPropertyInfoCache<Users>("Test", fakeType.Type));

				Assert.That(fakeType.Propertys.Count, Is.EqualTo(1));
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(6));
				Assert.That(fakeType.Attributes, Is.Empty);

				var refElement = fakeType.DefaultFactory();
				var originalValue = new Users();

				Assert.That(refElement, Is.Not.Null.And.TypeOf(fakeType.Type));

				var propTest = fakeType.Propertys["Test"];
				Assert.That(propTest, Is.Not.Null);
				propTest.Setter.Invoke(refElement, originalValue);
				var propValue = propTest.Getter.Invoke(refElement);
				Assert.That(propValue, Is.Not.Null.And.EqualTo(originalValue));
			}, Throws.Nothing);
		}

		[Test]
		public void FakeClassFunction()
		{
			var cache = new DbConfig(true);
			Assert.That(() =>
			{
				var fakeType = cache.GetFake("FakeType");

				fakeType.Mehtods.Add(new FakeMethodInfoCache((e, g) =>
				{
					return g[0] + (string) g[1];
				}, "TestMethod"));

				Assert.That(fakeType.Propertys, Is.Empty);
				Assert.That(fakeType.Mehtods.Count, Is.EqualTo(7));
				Assert.That(fakeType.Attributes, Is.Empty);

				var refElement = fakeType.DefaultFactory();

				Assert.That(refElement, Is.Not.Null.And.TypeOf(fakeType.Type));

				FakeMethodInfoCache propTest = (FakeMethodInfoCache)fakeType.Mehtods.FirstOrDefault(s => s.MethodName == "TestMethod");
				Assert.That(propTest, Is.Not.Null);
				var invoke = propTest.Invoke(refElement, "This is ", "an Test");
				Assert.That(invoke, Is.Not.Null.And.EqualTo("This is an Test"));
			}, Throws.Nothing);
		}
	}
}