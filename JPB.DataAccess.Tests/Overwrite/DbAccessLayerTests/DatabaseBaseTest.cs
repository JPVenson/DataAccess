#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.dotMemoryUnit;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.MetaAPI;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
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
	[TestFixture(DbAccessType.MsSql, false, false)]
	[TestFixture(DbAccessType.MsSql, false, true)]
	
	[TestFixture(DbAccessType.SqLite, true, false)]
	[TestFixture(DbAccessType.SqLite, false, false)]
	[TestFixture(DbAccessType.SqLite, false, true)]
	
	//[TestFixture(DbAccessType.MySql, true, false)]
	//[TestFixture(DbAccessType.MySql, false, false)]
	//[TestFixture(DbAccessType.MySql, false, true)]
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
			Measurements = new List<TimeMeasurement>();
		}

		class TimeMeasurement
		{
			private readonly string _key;
			private readonly TimeSpan _time;

			public TimeMeasurement(string key, TimeSpan time)
			{
				_key = key;
				_time = time;
			}

			public string Key
			{
				get { return _key; }
			}

			public TimeSpan Time
			{
				get { return _time; }
			}
		}

		private IList<TimeMeasurement> Measurements { get; set; }

		public void Measure(Action action, [CallerMemberName]string name = null)
		{
			Measure(() =>
			{
				action();
				return (object) null;
			}, name);
		}

		public T Measure<T>(Func<T> action, [CallerMemberName]string name = null)
		{
			var sw = new Stopwatch();
			sw.Start();
			var result = action();
			sw.Stop();
			Measurements.Add(new TimeMeasurement(name, sw.Elapsed));
			return result;
		}

		[TearDown]
		public void TestTearDown()
		{
			foreach (var timeMeasurement in Measurements)
			{
				TestContext.Out.WriteLine($"Measure: {timeMeasurement.Key} - '{timeMeasurement.Time:c}'");
			}

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