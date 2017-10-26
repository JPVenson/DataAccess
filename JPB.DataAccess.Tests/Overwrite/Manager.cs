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

namespace JPB.DataAccess.Tests
{
	public class Manager : IManager
	{
		public Manager()
		{
			_errorData = new StringBuilder();
			_managers = new ConcurrentDictionary<DbAccessType, Func<IManagerImplementation>>();
			_managers.Add(DbAccessType.MsSql, () => new MsSqlManager());
			_managers.Add(DbAccessType.SqLite, () => new SqLiteManager());
			_managers.Add(DbAccessType.MySql, () => new MySqlManager());
			AllTestContextHelper.TestSetup(null);
		}

		private readonly IDictionary<DbAccessType, Func<IManagerImplementation>> _managers;

		private StringBuilder _errorData;

		private IManagerImplementation _selectedMgr;

		public DbAccessLayer GetWrapper(DbAccessType type, params object[] additionalArguments)
		{
			DbAccessLayer expectWrapper = null;
			_errorData.AppendLine("---------------------------------------------");
			_errorData.AppendLine("Found " + type);

			Assert.That(new DbConfig().SClassInfoCaches, Is.Empty, () => "The Global Class cache is not empty");
			var testClassName = TestContext.CurrentContext.Test.ClassName.GetHashCode() + "_" +
			                    TestContext.CurrentContext.Test.MethodName.GetHashCode();
			var arguments =
					additionalArguments.Select(
					                           f =>
						                           f?.GetHashCode().ToString() ?? TestContext.CurrentContext.Random.NextShort().ToString())
					                   .ToArray();

			if (arguments.Any())
			{
				testClassName = testClassName + "_ARG_" + arguments.Aggregate((e, f) => e + f).GetHashCode();
			}
			testClassName = new Regex("[^a-zA-Z0-9]").Replace(testClassName, "_");
			_errorData.AppendLine($"Attach to Database: {testClassName}");

			expectWrapper = (_selectedMgr = _managers[type]()).GetWrapper(type, testClassName);
			expectWrapper.Config.EnableInstanceThreadSafety = true;


			expectWrapper.RaiseEvents = true;
			expectWrapper.OnSelect += (sender, eventArg) =>
			                          {
				                          _errorData.AppendFormat(@"SELECT: \r\n{0}", eventArg.QueryDebugger);
				                          _errorData.AppendLine();
			                          };

			expectWrapper.OnDelete += (sender, eventArg) =>
			                          {
				                          _errorData.AppendFormat(@"DELETE: \r\n{0}", eventArg.QueryDebugger);
				                          _errorData.AppendLine();
			                          };

			expectWrapper.OnInsert += (sender, eventArg) =>
			                          {
				                          _errorData.AppendFormat(@"INSERT: \r\n{0}", eventArg.QueryDebugger);
				                          _errorData.AppendLine();
			                          };

			expectWrapper.OnUpdate += (sender, eventArg) =>
			                          {
				                          _errorData.AppendFormat(@"UPDATE: \r\n{0}", eventArg.QueryDebugger);
				                          _errorData.AppendLine();
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
			TestContext.Error.WriteLine(_errorData.ToString());
			_errorData.Clear();

			_selectedMgr.FlushErrorData();
		}

		public void Clear()
		{
			_selectedMgr.Clear();
		}
	}
}