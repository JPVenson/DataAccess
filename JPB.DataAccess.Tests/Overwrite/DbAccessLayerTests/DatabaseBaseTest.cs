#region

using System;
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

	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
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

		protected DatabaseBaseTest(DbAccessType type, params object[] additionalArguments)
		{
			AdditionalArguments = additionalArguments;
			Type = type;
		}

		public DbAccessLayer DbAccess
		{
			get { return _dbAccess ?? (_dbAccess = Mgr.GetWrapper(Type, AdditionalArguments)); }
			private set { _dbAccess = value; }
		}

		public IManager Mgr { get; private set; }
		public DbAccessType Type { get; }
	}
}