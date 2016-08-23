﻿using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.PagerTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	public class PagerConstraintTests
	{
		private readonly DbAccessType _type;

		public PagerConstraintTests(DbAccessType type)
		{
			_type = type;
		}

		private DbAccessLayer _dbAccess;
		private IManager _mgr;

		[SetUp]
		public void Init()
		{
			_mgr = new Manager();
			_dbAccess = _mgr.GetWrapper(_type);
		}

		[Test]
		public void CurrentPageBiggerOrEqualsOne()
		{
			var dataPager = _dbAccess.Database.CreatePager<Users>();
			Assert.That(() => dataPager.CurrentPage, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.PageSize, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.CurrentPage = 0, Throws.Exception);
		}
	}
}
