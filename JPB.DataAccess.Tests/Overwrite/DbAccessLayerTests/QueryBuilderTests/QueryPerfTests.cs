using System;
using System.Diagnostics;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests.QueryBuilderTests
{
	[TestFixture]
	[Explicit("Long running Performance tests")]
	public class QueryPerfTests
	{
		public QueryPerfTests()
		{

		}

		[Test]
		[TestCase(10)]
		[TestCase(100)]
		[TestCase(1000)]
		public void CreateNStatements(int runs)
		{
			var timeForEach = new Stopwatch();
			for (int i = 0; i < runs; i++)
			{
				var dbaAccess = new DbAccessLayer(DbAccessType.MsSql, "");
				timeForEach.Start();
				var query = dbaAccess.Query().Select.Table<Users>().Where.Column(f => f.UserID).Is.EqualsTo(10);
				timeForEach.Stop();
			}

			TestContext.Out.WriteLine($"Test took {timeForEach.Elapsed}");
			TestContext.Out.WriteLine($"That is {TimeSpan.FromMilliseconds(timeForEach.Elapsed.TotalMilliseconds / (runs * 1D))} per call");
		}
	}
}
