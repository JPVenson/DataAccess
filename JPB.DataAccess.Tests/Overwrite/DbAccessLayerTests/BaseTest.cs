#region

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using NUnit.Framework;

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
	public abstract class BaseTest
	{
		[SetUp]
		public void Init()
		{
			Mgr = new Manager();
		}

		protected IDisposable MakeManager(DbAccessType type, params object[] arguments)
		{
			DbAccess = Mgr.GetWrapper(Type, arguments);
			return new ManagerScope(() =>
			{
				this.TearDown();
				_dbAccess.Remove(DbAccess);
			});
		}

		[TearDown]
		public void TestTearDown()
		{
			this.TearDown();
		}

		[SetUp]
		public void Clear()
		{
			this.ClearDb();
		}

		public object[] AdditionalArguments { get; }
		private readonly List<DbAccessLayer> _dbAccess = new List<DbAccessLayer>();

		protected BaseTest(DbAccessType type, params object[] additionalArguments)
		{
			AdditionalArguments = additionalArguments;
			Type = type;
		}

		public DbAccessLayer DbAccess
		{
			get
			{
				var last = _dbAccess.LastOrDefault();
				if (last == null)
				{
					MakeManager(Type, AdditionalArguments);
				}
				return last;
			}
			private set { _dbAccess.Add(value); }
		}

		public IManager Mgr { get; private set; }
		public DbAccessType Type { get; }
	}
}