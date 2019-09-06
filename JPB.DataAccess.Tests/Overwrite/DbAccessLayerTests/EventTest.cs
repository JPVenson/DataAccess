#region

using System;
using System.Threading;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.None)]
	[Explicit]
	public class EventTest : DatabaseBaseTest
	{
		public EventTest(DbAccessType type, bool asyncExecution, bool syncronised) : base(type, asyncExecution, syncronised)
		{
		}

		public void TestEvent(Action<DatabaseActionHandler> eventType, Action shouldRaiseEvent, bool shouldInvoke)
		{
			var riseFlag = false;
			var eventDone = new AutoResetEvent(false);
			DatabaseActionHandler handler = (sender, eventx) =>
			{
				Assert.That(riseFlag, Is.False,
					"The wrong event or the wrong ammount of events are risen. Raise: " + DbAccess.RaiseEvents);
				riseFlag = true;
				eventDone.Set();
			};
			var setState = new OnDatabaseActionHandler((sender, evenat, handlerDone) =>
			{
				eventDone.Set();
			});

			try
			{
				DbAccess.RaiseEvents = true;
				DbAccess.HandlerRaised += setState;
				eventType(handler);
				shouldRaiseEvent();
				Assert.That(eventDone.WaitOne(TimeSpan.FromSeconds(20)), Is.True, "The eventhandler did not respond in a timely manner");
				Assert.That(riseFlag, Is.EqualTo(shouldInvoke), "First call should be succeed but did not");

				DbAccess.RaiseEvents = false;
				riseFlag = false;
				shouldRaiseEvent();
				Assert.That(riseFlag, Is.EqualTo(false), "Last call should not succeed but did");
			}
			finally
			{
				DbAccess.OnSelect -= handler;
				DbAccess.OnDelete -= handler;
				DbAccess.OnInsert -= handler;
				DbAccess.OnUpdate -= handler;
				DbAccess.HandlerRaised -= setState;
			}
		}
		
		[Test]
		public void TestOnDelete()
		{
			TestEvent(evtArg => DbAccess.OnDelete += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent(evtArg => DbAccess.OnDelete += evtArg, () => entity = DbAccess.InsertWithSelect(new Users()), false);
			TestEvent(evtArg => DbAccess.OnDelete += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent(evtArg => DbAccess.OnDelete += evtArg, () => DbAccess.Update(entity), false);
			TestEvent(evtArg => DbAccess.OnDelete += evtArg, () => DbAccess.Delete(entity), true);
		}

		[Test]
		public void TestOnInsert()
		{
			TestEvent(evtArg => DbAccess.OnInsert += evtArg, () => DbAccess.Insert(new Users()), true);
			Users entity = null;
			TestEvent(evtArg => DbAccess.OnInsert += evtArg,
			() => entity = DbAccess.InsertWithSelect(new Users()), true);
			TestEvent(evtArg => DbAccess.OnInsert += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent(evtArg => DbAccess.OnInsert += evtArg, () => DbAccess.Update(entity), false);
			TestEvent(evtArg => DbAccess.OnInsert += evtArg, () => DbAccess.Delete(entity), false);
		}

		[Test]
		public void TestOnSelect()
		{
			TestEvent(evtArg => DbAccess.OnSelect += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent(evtArg => DbAccess.OnSelect += evtArg, () => entity = DbAccess.InsertWithSelect(new Users()), true);
			TestEvent(evtArg => DbAccess.OnSelect += evtArg, () => DbAccess.Select<Users>(), true);
			TestEvent(evtArg => DbAccess.OnSelect += evtArg, () => DbAccess.Update(entity), false);
			TestEvent(evtArg => DbAccess.OnSelect += evtArg, () => DbAccess.Delete(entity), false);
		}

		[Test]
		public void TestOnUpdate()
		{
			TestEvent(evtArg => DbAccess.OnUpdate += evtArg, () => DbAccess.Insert(new Users()), false);
			Users entity = null;
			TestEvent(evtArg => DbAccess.OnUpdate += evtArg,
			() => entity = DbAccess.InsertWithSelect(new Users()), false);
			TestEvent(evtArg => DbAccess.OnUpdate += evtArg, () => DbAccess.Select<Users>(), false);
			TestEvent(evtArg => DbAccess.OnUpdate += evtArg, () => DbAccess.Update(entity), true);
			TestEvent(evtArg => DbAccess.OnUpdate += evtArg, () => DbAccess.Delete(entity), false);
		}
	}
}