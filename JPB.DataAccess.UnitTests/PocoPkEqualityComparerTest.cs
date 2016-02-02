using System;
using JPB.DataAccess.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1;

namespace JPB.DataAccess.UnitTests
{
	[TestClass]
	public class PocoPkEqualityComparerTest
	{
		[TestMethod]
		public void Instance()
		{
			var comparer = new PocoPkComparer<Users>();
		}

		[TestMethod]
		public void ComparesonInstanceNull()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsFalse(comparer.Equals(null, new Users()));
			Assert.IsFalse(comparer.Equals(new Users(), null));
		}

		[TestMethod]
		public void ComparesonInstanceEquals()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsFalse(comparer.Equals(new Users() { User_ID = 23 }, new Users() { User_ID = 4234 }));
			var sharedInstance = new Users();
			Assert.IsTrue(comparer.Equals(sharedInstance, sharedInstance));
		}

		[TestMethod]
		public void ComparesonPropEqualsAssertionLong()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsFalse(comparer.Equals(new Users() { User_ID = 5 }, new Users()));
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void ComparesonPropEqualsAssertionInt()
		{
			var comparer = new PocoPkComparer<Users>(5);
			Assert.IsFalse(comparer.Equals(new Users() { User_ID = 5 }, new Users()));
		}

		[TestMethod]
		public void ComparesonPropEqualsAssertionIntTrue()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsFalse(comparer.Equals(new Users() { User_ID = 5 }, new Users() { User_ID = 5 }));
		}

		[TestMethod]
		public void ComparesonPropEquals()
		{
			var comparer = new PocoPkComparer<Users>();
			Assert.IsTrue(comparer.Equals(new Users() { User_ID = 8 }, new Users() { User_ID = 8 }));
		}

		[TestMethod]
		public void ComparesonPropEqualsWithAssertion()
		{
			var comparer = new PocoPkComparer<Users>(5L);
			Assert.IsTrue(comparer.Equals(new Users() { User_ID = 8 }, new Users() { User_ID = 8 }));
		}
	}
}
