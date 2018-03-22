#region

using System;
using System.Collections.Generic;
using System.Linq;
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
	public abstract class DatabaseBaseTest
	{
		[SetUp]
		public void Init()
		{
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
			Assert.That(failed, Is.False, () => "Invalid Connection State");
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