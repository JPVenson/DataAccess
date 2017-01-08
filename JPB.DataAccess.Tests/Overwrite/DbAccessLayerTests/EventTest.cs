using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	//[TestFixture(DbAccessType.MySql)]
	public class EventTest
	{
		private readonly DbAccessType _type;

		public EventTest(DbAccessType type)
		{
			_type = type;
		}

		private DbAccessLayer _dbAccess;
		private IManager _mgr;

		[SetUp]
		public void Init()
		{
			_mgr = new Manager();
			_dbAccess = _mgr.GetWrapper(_type);
		}

		[TearDown]
		public void TestTearDown()
		{
			// inc. class name
			var fullNameOfTheMethod = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			// method name only
			var methodName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
			// the state of the test execution
			var state = NUnit.Framework.TestContext.CurrentContext.Result.Outcome == ResultState.Failure; // TestState enum

			if (state)
			{
				_mgr.FlushErrorData();
			}
		}

		[SetUp]
		public void Clear()
		{
			_dbAccess.Config.Dispose();
			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			if (_dbAccess.DbAccessType == DbAccessType.MsSql)
				_dbAccess.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
		}

		public void TestOnUpdate()
		{
			_dbAccess.RaiseEvents = true;
			var insertWithSelect = _dbAccess.InsertWithSelect(new Users());

			var riseFlag = false;
			_dbAccess.OnUpdate += (sender, eventx) =>
			{
				riseFlag = true;
			};
			_dbAccess.Update(insertWithSelect);
			Assert.True(riseFlag);
			_dbAccess.RaiseEvents = false;
			riseFlag = false;
			_dbAccess.Update(insertWithSelect);
			Assert.False(riseFlag);
		}

		public void TestOnInsert()
		{
			_dbAccess.RaiseEvents = true;
			var riseFlag = false;
			_dbAccess.OnInsert += (sender, eventx) =>
			{
				riseFlag = true;
			};
			_dbAccess.Insert(new Users());
			Assert.True(riseFlag);

			_dbAccess.RaiseEvents = false;
			riseFlag = false;
			_dbAccess.Insert(new Users());
			Assert.False(riseFlag);

		}

		public void TestOnSelect()
		{
			_dbAccess.RaiseEvents = true;

		}

		public void TestOnDelete()
		{
			_dbAccess.RaiseEvents = true;

		}
	}
}
