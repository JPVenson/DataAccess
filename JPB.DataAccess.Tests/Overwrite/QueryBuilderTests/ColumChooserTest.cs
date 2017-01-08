using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.Operators.Selection;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.Overwrite.QueryBuilderTests
{
	[TestFixture()]
	public class ColumChooserTest
	{
		public ColumChooserTest()
		{
			_fakeDb = new RootQuery(new DbAccessLayer());
		}

		private const string Prefix = "s";
		private IQueryBuilder _fakeDb;

		[Test]
		public void CheckPropertyLamda()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), null);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column(f => f.UserID);
			Assert.That(cc2._columns, Contains.Item(UsersMeta.PrimaryKeyName));
		}

		[Test]
		public void CheckPropertyDirect()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), null);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column(UsersMeta.PrimaryKeyName);
			Assert.That(cc2._columns, Contains.Item(UsersMeta.PrimaryKeyName));
		}

		[Test]
		public void CheckPropertyInvalidValue()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), null);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column("xxx");
			Assert.That(cc2._columns, Contains.Item("xxx"));
		}


		[Test]
		public void CheckPropertyLamdaWithPrefix()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), Prefix);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column(f => f.UserID);
			Assert.That(cc2._columns, Contains.Item(Prefix + "." +UsersMeta.PrimaryKeyName));
		}

		[Test]
		public void CheckPropertyDirectPrefix()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), Prefix);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column(UsersMeta.PrimaryKeyName);
			Assert.That(cc2._columns, Contains.Item(Prefix + "." + UsersMeta.PrimaryKeyName));
		}

		[Test]
		public void CheckPropertyInvalidValuePrefix()
		{
			var cc = new ColumnChooser<Users>(_fakeDb, new List<string>(), Prefix);
			Assert.That(cc.Cache, Is.Not.Null);
			Assert.That(cc._columns, Is.Empty);
			var cc2 = cc.Column("xxx");
			Assert.That(cc2._columns, Contains.Item(Prefix + ".xxx"));
		}
	}
}
