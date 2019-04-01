#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite
{
	public class Manager : IManager
	{
		private readonly IDictionary<DbAccessType, Func<IManagerImplementation>> _managers;

		private readonly StringBuilder _errorData;

		private readonly List<KeyValuePair<string, string>> _querys = new List<KeyValuePair<string, string>>();

		private IManagerImplementation _selectedMgr;

		public Manager()
		{
			_errorData = new StringBuilder();
			_managers = new ConcurrentDictionary<DbAccessType, Func<IManagerImplementation>>();
			_managers.Add(DbAccessType.MsSql, () => new MsSqlManager());
			_managers.Add(DbAccessType.SqLite, () => new SqLiteManager());
			_managers.Add(DbAccessType.MySql, () => new MySqlManager());
			AllTestContextHelper.TestSetup(null);
		}

		public DbAccessLayer GetWrapper(DbAccessType type, params object[] additionalArguments)
		{
			DbAccessLayer expectWrapper = null;
			_errorData.AppendLine("---------------------------------------------");
			_errorData.AppendLine("Found " + type);

			Assert.That(new DbConfig().SClassInfoCaches, Is.Empty, () => "The Global Class cache is not empty");
			var testClassName = TestContext.CurrentContext.Test.ClassName.Replace(typeof(Manager).Namespace, "")
									.Where(e => char.IsUpper(e)).Select(e => e.ToString())
									.Aggregate((e, f) => e + f) + "." +
								TestContext.CurrentContext.Test.MethodName;

			testClassName = testClassName + "_" + Guid.NewGuid().ToString("N");
			testClassName = new Regex("[^a-zA-Z0-9]").Replace(testClassName, "_");
			_errorData.AppendLine($"Attach to Database: {testClassName}");

			expectWrapper = (_selectedMgr = _managers[type]()).GetWrapper(type, testClassName);
			expectWrapper.Config.EnableInstanceThreadSafety = true;


			expectWrapper.RaiseEvents = true;
			expectWrapper.RaiseEventsAsync = false;
			expectWrapper.OnSelect += (sender, eventArg) =>
			{
				_querys.Add(new KeyValuePair<string, string>("SELECT", eventArg.QueryDebugger.ToString()));
			};

			expectWrapper.OnDelete += (sender, eventArg) =>
			{
				_querys.Add(new KeyValuePair<string, string>("DELETE", eventArg.QueryDebugger.ToString()));
			};

			expectWrapper.OnInsert += (sender, eventArg) =>
			{
				_querys.Add(new KeyValuePair<string, string>("INSERT", eventArg.QueryDebugger.ToString()));
			};

			expectWrapper.OnUpdate += (sender, eventArg) =>
			{
				_querys.Add(new KeyValuePair<string, string>("UPDATE", eventArg.QueryDebugger.ToString()));
			};

			expectWrapper.OnNonResultQuery += (sender, eventArg) =>
			{
				_querys.Add(new KeyValuePair<string, string>("Query", eventArg.QueryDebugger.ToString()));
			};

			expectWrapper.OnFailedQuery += (sender, eventArg, exception) =>
			{
				_querys.Add(new KeyValuePair<string, string>("Query Failed", eventArg.QueryDebugger.ToString()));
			};

			Assert.NotNull(expectWrapper, "This test cannot run as no Database Variable is defined");
			var checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);
			AllTestContextHelper.TestSetup(expectWrapper.Config.ConstructorSettings);
			expectWrapper.Multipath = true;
			return expectWrapper;
		}

		public void FlushErrorData()
		{
			_querys.Reverse();
			foreach (var keyValuePair in _querys)
			{
				_errorData.AppendLine($"{keyValuePair.Key}");
				_errorData.AppendLine($"{keyValuePair.Value}");
				_errorData.AppendLine();
			}

			TestContext.Error.WriteLine(_errorData.ToString());
			_errorData.Clear();

			_selectedMgr?.FlushErrorData();
		}

		public void Clear()
		{
			if (_selectedMgr != null)
			{
				_selectedMgr.Clear();
			}
		}
	}
}