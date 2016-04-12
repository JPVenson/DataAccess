using System;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.PocoPkEquallityTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class PocoPkEqualityComparerTest
	{
		[Test]
		public void ComparesonInstanceEquals()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsFalse(comparer.Equals(new Users {UserID = 23}, new Users { UserID = 4234}));
			var sharedInstance = new Users();
			Assert.IsTrue(comparer.Equals(sharedInstance, sharedInstance));
		}

		[Test]
		public void ComparesonInstanceNull()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsFalse(comparer.Equals(null, new Users()));
			Assert.IsFalse(comparer.Equals(new Users(), null));
		}

		[Test]
		public void ComparesonPropEquals()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsTrue(comparer.Equals(new Users { UserID = 8}, new Users { UserID = 8}));
		}

		[Test]
		public void ComparesonPropEqualsAssertionInt()
		{
			Assert.That(() => new PocoPkComparer<Users>(5), Throws.Exception.TypeOf<NotSupportedException>());
		}

		[Test]
		public void ComparesonPropEqualsAssertionIntTrue()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsFalse(comparer.Equals(new Users { UserID = 5}, new Users { UserID = 5}));
		}

		[Test]
		public void ComparesonPropEqualsAssertionLong()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsFalse(comparer.Equals(new Users { UserID = 5}, new Users()));
		}

		[Test]
		public void ComparesonPropEqualsWithAssertion()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsTrue(comparer.Equals(new Users { UserID = 8}, new Users { UserID = 8}));
		}

		[Test]
		public void Instance()
		{
			var comparer = new PocoPkComparer<Users>();
		}
	}
}