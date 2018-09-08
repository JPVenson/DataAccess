using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using NUnit.Framework;

[SetUpFixture]
// ReSharper disable once CheckNamespace
public class GlobalTestContext
{
	public GlobalTestContext()
	{
		
	}

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
	}

	[OneTimeTearDown]
	public void RunAfterAnyTests()
	{
	}
}