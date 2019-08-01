using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Framework.DbInfoConfig;
using NUnit.Framework;

namespace JPB.DataAccess.Tests
{
	[SetUpFixture]
// ReSharper disable once CheckNamespace
	public class GlobalTestContext
	{
		public GlobalTestContext()
		{
			SetUpActions = new List<Action>();
			TearDowns = new List<Action>();
			var generatedObjects = new ConcurrentDictionary<Type, object>();

			foreach (var fixture in typeof(GlobalTestContext).Assembly.GetTypes()
				.Where(f => f.GetCustomAttribute(typeof(SetUpFixtureAttribute)) != null)
				.Where(e => e != typeof(GlobalTestContext)))
			{
				var setup = fixture.GetMethods().Where(e => e.GetCustomAttribute(typeof(OneTimeSetUpAttribute)) != null);
				var tearDown = fixture.GetMethods().Where(e => e.GetCustomAttribute(typeof(OneTimeTearDownAttribute)) != null);

				foreach (var methodInfo in setup)
				{
					SetUpActions.Add(() => methodInfo.Invoke(generatedObjects.GetOrAdd(fixture, Activator.CreateInstance), null));
				}
				foreach (var methodInfo in tearDown)
				{
					TearDowns.Add(() => methodInfo.Invoke(generatedObjects.GetOrAdd(fixture, Activator.CreateInstance), null));
				}
			}
		}

		public List<Action> SetUpActions { get; set; }
		public List<Action> TearDowns { get; set; }

		/// <summary>
		/// Runs the before any tests.
		/// This should remove the created artifacts from old runs
		/// </summary>
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			TestContext.Out.WriteLine("Removing old Pessimistic Created Dlls");
			foreach (var listPessimisticCreatedDll in FactoryHelper.ListPessimisticCreatedDlls())
			{
				TestContext.Out.WriteLine("Remove: " + listPessimisticCreatedDll);
				File.Delete(listPessimisticCreatedDll);
			}

			foreach (var upAction in SetUpActions)
			{
				upAction();
			}
		}

		[OneTimeTearDown]
		public void RunAfterAnyTests()
		{
			foreach (var upAction in TearDowns)
			{
				upAction();
			}
		}
	}
}