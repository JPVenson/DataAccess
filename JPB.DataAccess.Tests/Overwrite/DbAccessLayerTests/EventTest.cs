#region

using System;
using JPB.DataAccess.DbEventArgs;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.None)]
	public class EventTest : DatabaseBaseTest
	{
		public EventTest(DbAccessType type) : base(type)
		{
		}

		public void TestEvent(Action<DatabaseActionHandler> eventType, Action shouldRaiseEvent, bool shouldInvoke)
		{
			DbAccess.RaiseEvents = true;
			var riseFlag = false;
			DatabaseActionHandler handler = (sender, eventx) =>
			{
				Assert.That(riseFlag, Is.False, "The wrong event or the wrong ammount of events are risen");
				riseFlag = true;
			};
			eventType(handler);
			shouldRaiseEvent();
			Assert.That(riseFlag, Is.EqualTo(shouldInvoke), "First call should be succeed but did not");

			DbAccess.RaiseEvents = false;
			riseFlag = false;
			shouldRaiseEvent();
			Assert.That(riseFlag, Is.EqualTo(false), "Last call should not succeed but did");

			DbAccess.OnSelect -= handler;
			DbAccess.OnDelete -= handler;
			DbAccess.OnInsert -= handler;
			DbAccess.OnUpdate -= handler;
		}

		[Test]
		public void TestOnSelect()
		{
			TestEvent((evtArg) => DbAccess.OnSelect += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent((evtArg) => DbAccess.OnSelect += evtArg, () => entity = DbAccess.InsertWithSelect(new Users()), true);
			TestEvent((evtArg) => DbAccess.OnSelect += evtArg, () => DbAccess.Select<Users>(), true);
			TestEvent((evtArg) => DbAccess.OnSelect += evtArg, () => DbAccess.Update(entity), false);
			TestEvent((evtArg) => DbAccess.OnSelect += evtArg, () => DbAccess.Delete(entity), false);
		}

		[Test]
		public void TestOnDelete()
		{
			TestEvent((evtArg) => DbAccess.OnDelete += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent((evtArg) => DbAccess.OnDelete += evtArg, () => entity = DbAccess.InsertWithSelect(new Users()), false);
			TestEvent((evtArg) => DbAccess.OnDelete += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent((evtArg) => DbAccess.OnDelete += evtArg, () => DbAccess.Update(entity), false);
			TestEvent((evtArg) => DbAccess.OnDelete += evtArg, () => DbAccess.Delete(entity), true);
		}

		[Test]
		public void TestOnInsert()
		{
			TestEvent((evtArg) => DbAccess.OnInsert += evtArg, () => DbAccess.Insert(new Users()), true);
			Users entity = null;
			TestEvent((evtArg) => DbAccess.OnInsert += evtArg, 
			() => entity = DbAccess.InsertWithSelect(new Users()), true);
			TestEvent((evtArg) => DbAccess.OnInsert += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent((evtArg) => DbAccess.OnInsert += evtArg, () => DbAccess.Update(entity), false);
			TestEvent((evtArg) => DbAccess.OnInsert += evtArg, () => DbAccess.Delete(entity), false);
		}

		[Test]
		public void TestOnUpdate()
		{
			TestEvent((evtArg) => DbAccess.OnUpdate += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent((evtArg) => DbAccess.OnUpdate += evtArg, 
			() => entity = DbAccess.InsertWithSelect(new Users()), false);
			TestEvent((evtArg) => DbAccess.OnUpdate += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent((evtArg) => DbAccess.OnUpdate += evtArg, () => DbAccess.Update(entity), true);
			TestEvent((evtArg) => DbAccess.OnUpdate += evtArg, () => DbAccess.Delete(entity), false);
		}
	}
}