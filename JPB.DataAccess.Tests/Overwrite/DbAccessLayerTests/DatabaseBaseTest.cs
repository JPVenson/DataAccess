#region

using System;
using System.Linq;
using JetBrains.dotMemoryUnit;
using JPB.DataAccess.Manager;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	public class ManagerScope : IDisposable
	{
		private readonly Action _then;

		public ManagerScope(Action then)
		{
			_then = then;
		}

		public void Dispose()
		{
			_then();
		}
	}

	[TestFixture(DbAccessType.MsSql, true, false)]
	[TestFixture(DbAccessType.SqLite, true, false)]
	[TestFixture(DbAccessType.MsSql, false, false)]
	[TestFixture(DbAccessType.SqLite, false, false)]
	[TestFixture(DbAccessType.MsSql, false, true)]
	[TestFixture(DbAccessType.SqLite, false, true)]
	[DotMemoryUnit(SavingStrategy = SavingStrategy.Never, FailIfRunWithoutSupport = false)]
	public abstract class DatabaseBaseTest
	{
		//MemoryCheckPoint memoryCheckPoint;
		[SetUp]
		public void Init()
		{
			//Warn.If(dotMemoryApi.IsEnabled, () => "WARNING DOTMEMORY IS NOT ENABLED");
			//memoryCheckPoint = dotMemory.Check();
			Mgr = new Manager();
		}

		[TearDown]
		public void TestTearDown()
		{
			var failed = false;
			if (_dbAccess != null)
			{
				if (_dbAccess.Database.ConnectionController.InstanceCounter != 0)
				{
					TestContext.Error.WriteLine("Invalid State Detected. Some connections are Still open. Proceed with Cleanup");
					failed = true;
				}
			}

			if (Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Failure) ||
				Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Error))
			{
				Mgr?.FlushErrorData();
			}
			else
			{
				Mgr?.Clear();
			}

			Mgr = null;
			DbAccess = null;
			//ThreadConnection.UseTransactionCurrent = false;
			Assert.That(failed, Is.False, () => "Invalid Connection State");

			//dotMemory.Check(mem =>
			//{
			//	Assert.That(mem.GetObjects(e => e.Type.Is<DbAccessLayer>()).ObjectsCount, Is.Zero);
			//	Assert.That(mem.GetObjects(e => e.LeakedOnEventHandler()).ObjectsCount, Is.Zero);
			//	Assert.That(mem.GetObjects(e => e.Interface.Is<IDbConnection>()).ObjectsCount, Is.Zero);
			//	Assert.That(mem.GetObjects(e => e.Interface.Is<IDbCommand>()).ObjectsCount, Is.Zero);
			//});
		}

		public object[] AdditionalArguments { get; }
		private DbAccessLayer _dbAccess;

		protected DatabaseBaseTest(DbAccessType type, bool asyncExecution, bool syncronised, params object[] additionalArguments)
		{
			TestContext.CurrentContext.Test.Properties.Add("Type", type);
			AdditionalArguments = additionalArguments.Concat(new[] { asyncExecution ? "1" : "0", Synronised ? "1" : "0" }).ToArray();
			Type = type;
			AsyncExecution = asyncExecution;
			Synronised = syncronised;
		}

		public bool Synronised { get; set; }

		public DbAccessLayer DbAccess
		{
			get
			{
				if (_dbAccess == null)
				{
					_dbAccess = Mgr.GetWrapper(Type, AdditionalArguments);
					_dbAccess.Async = AsyncExecution;
					_dbAccess.ThreadSave = Synronised;
				}

				return _dbAccess;
			}
			private set { _dbAccess = value; }
		}

		public IManager Mgr { get; private set; }
		public DbAccessType Type { get; }
		public bool AsyncExecution { get; }
	}
}