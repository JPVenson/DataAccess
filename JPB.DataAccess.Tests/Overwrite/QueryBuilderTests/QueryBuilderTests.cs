using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base;
using JPB.DataAccess.Tests.Overwrite;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.QueryBuilderTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class QueryBuilderTests
	{
		public QueryBuilderTests()
		{
			
		}

		[SetUp]
		public void Init()
		{
			DbAccessLayer = new Manager().GetWrapper();
		}

		public DbAccessLayer DbAccessLayer { get; set; }

		[Test]
		public void Select()
		{
			DataMigrationHelper.AddUsers(250);

			var runPrimetivSelect = DbAccessLayer.RunPrimetivSelect<int>("SELECT COUNT(1) FROM Users")[0];
			var deSelect = DbAccessLayer.Select<Users>();
			var forResult = DbAccessLayer.Query().Select<Users>().ForResult().ToArray();

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult.Count()));
			Assert.That(deSelect.Length, Is.EqualTo(forResult.Count()));

			for (int index = 0; index < forResult.Length; index++)
			{
				var userse = forResult[index];
				var userbe = deSelect[index];
				Assert.That(userse.UserID, Is.EqualTo(userbe.UserID));
				Assert.That(userse.UserName, Is.EqualTo(userbe.UserName));
			}
		}

		[Test]
		public void Count()
		{
			DataMigrationHelper.AddUsers(250);

			var runPrimetivSelect = DbAccessLayer.RunPrimetivSelect<int>("SELECT COUNT(1) FROM Users")[0];
			var deSelect = DbAccessLayer.Select<Users>();
			var forResult = DbAccessLayer.Query().Select<Users>().CountInt().ForResult().FirstOrDefault();

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult));
			Assert.That(deSelect.Length, Is.EqualTo(forResult));
		}
	}
}
