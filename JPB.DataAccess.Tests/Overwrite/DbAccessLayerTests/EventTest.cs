#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.None)]
	public class EventTest : BaseTest
	{
		public EventTest(DbAccessType type) : base(type)
		{
		}

		public void TestOnSelect()
		{
			DbAccess.RaiseEvents = true;
		}

		public void TestOnDelete()
		{
			DbAccess.RaiseEvents = true;
		}

		[Test]
		public void TestOnInsert()
		{
			DbAccess.RaiseEvents = true;
			var riseFlag = false;
			DbAccess.OnInsert += (sender, eventx) => { riseFlag = true; };
			DbAccess.Insert(new Users());
			Assert.True(riseFlag);

			DbAccess.RaiseEvents = false;
			riseFlag = false;
			DbAccess.Insert(new Users());
			Assert.False(riseFlag);
		}

		[Test]
		public void TestOnUpdate()
		{
			DbAccess.RaiseEvents = true;
			var insertWithSelect = DbAccess.InsertWithSelect(new Users());

			var riseFlag = false;
			DbAccess.OnUpdate += (sender, eventx) => { riseFlag = true; };
			DbAccess.Update(insertWithSelect);
			Assert.True(riseFlag);
			DbAccess.RaiseEvents = false;
			riseFlag = false;
			DbAccess.Update(insertWithSelect);
			Assert.False(riseFlag);
		}
	}
}