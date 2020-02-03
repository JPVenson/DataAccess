#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using JPB.DataAccess.Tests.TestFramework;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class ProcedureTests : DatabaseBaseTest
	{

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void ProcedureDirectParamTest()
		{
			DataMigrationHelper.AddUsersFast(100, DbAccess);

			Assert.That(() => DbAccess.Select<TestProcBParamsDirect>(new object[] {10}),
				Is.Not.Null.And.Property("Length").EqualTo(9));
		}

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void ProcedureParamLessTest()
		{
			DataMigrationHelper.AddUsersFast(100, DbAccess);
			var expectedUser = DbAccess.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

			Assert.IsNotNull(expectedUser);
			Assert.AreNotEqual(expectedUser.Length, 0);

			var refSelect =
				DbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.TableName)));
			Assert.AreEqual(expectedUser.Length, refSelect);
		}

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void ProcedureParamTest()
		{
			DataMigrationHelper.AddUsersFast(100, DbAccess);

			Assert.That(() => DbAccess.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams
			{
				Number = 10
			}), Is.Not.Null.And.Property("Length").EqualTo(9));
		}

		/// <inheritdoc />
		public ProcedureTests(DbAccessType type, bool asyncExecution ,bool syncronised) : base(type, asyncExecution, syncronised)
		{
		}
	}
}