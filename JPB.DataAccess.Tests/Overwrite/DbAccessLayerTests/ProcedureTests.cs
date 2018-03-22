#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class ProcedureTests : DatabaseBaseTest
	{
		public ProcedureTests(DbAccessType type) : base(type)
		{
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureDirectParamTest()
		{
			if (DbAccess.DbAccessType != DbAccessType.MsSql)
			{
				return;
			}
			DataMigrationHelper.AddUsers(100, DbAccess);

			Assert.That(() => DbAccess.Select<TestProcBParamsDirect>(new object[] {10}),
				Is.Not.Null.And.Property("Length").EqualTo(9));
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureParamLessTest()
		{
			if (DbAccess.DbAccessType != DbAccessType.MsSql)
			{
				return;
			}
			DataMigrationHelper.AddUsers(100, DbAccess);
			var expectedUser = DbAccess.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

			Assert.IsNotNull(expectedUser);
			Assert.AreNotEqual(expectedUser.Length, 0);

			var refSelect =
				DbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.TableName)));
			Assert.AreEqual(expectedUser.Length, refSelect);
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureParamTest()
		{
			if (DbAccess.DbAccessType != DbAccessType.MsSql)
			{
				return;
			}
			DataMigrationHelper.AddUsers(100, DbAccess);

			Assert.That(() => DbAccess.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams
			{
				Number = 10
			}), Is.Not.Null.And.Property("Length").EqualTo(9));
		}
	}
}