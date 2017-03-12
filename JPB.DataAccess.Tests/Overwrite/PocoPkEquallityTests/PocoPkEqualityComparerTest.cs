using System;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.PocoPkEquallityTests

{
    [TestFixture]
    public class PocoPkEqualityComparerTest
    {
        [Test]
        public void ComparesonInstanceEquals()
        {
            var comparer = new PocoPkComparer<Users>();
            Assert.That(comparer.Equals(new Users {UserID = 23}, new Users {UserID = 4234}), Is.False);
            var sharedInstance = new Users();
            Assert.That(comparer.Equals(sharedInstance, sharedInstance), Is.True);
        }

        [Test]
        public void ComparesonInstanceNull()
        {
            var comparer = new PocoPkComparer<Users>();
            Assert.That(comparer.Equals(null, new Users()), Is.False);
            Assert.That(comparer.Equals(new Users(), null), Is.False);
            Assert.That(comparer.Equals(null, null), Is.True);
        }

        [Test]
        public void ComparesonPropEquals()
        {
            var comparer = new PocoPkComparer<Users>();
            Assert.That(comparer.Equals(new Users {UserID = 8}, new Users {UserID = 8}), Is.True);
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
            Assert.That(comparer.Equals(new Users {UserID = 5}, new Users {UserID = 5}), Is.False);
        }

        [Test]
        public void ComparesonPropEqualsAssertionLong()
        {
            var comparer = new PocoPkComparer<Users>(5L);
            Assert.That(comparer.Equals(new Users {UserID = 5}, new Users()), Is.False);
        }

        [Test]
        public void ComparesonPropEqualsWithAssertion()
        {
            var comparer = new PocoPkComparer<Users>(5L);
            Assert.That(comparer.Equals(new Users {UserID = 8}, new Users {UserID = 8}), Is.True);
        }

        [Test]
        public void Instance()
        {
            Assert.That(() => { new PocoPkComparer<Users>(); }, Throws.Nothing);
        }
    }
}