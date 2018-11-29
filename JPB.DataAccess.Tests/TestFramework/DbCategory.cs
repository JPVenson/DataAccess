using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace JPB.DataAccess.Tests.TestFramework
{
	/// <summary>
	///		Returns the list of Standard variations that any test must accept
	/// </summary>
	/// <seealso cref="System.Collections.IEnumerable" />
	public class StandardDatabaseTests : IEnumerable
	{
		public StandardDatabaseTests()
		{
			DbAccessTypes = new[] { DbAccessType.MsSql, DbAccessType.SqLite, DbAccessType.MySql };
		}

		public DbAccessType[] DbAccessTypes { get; set; }

		public IEnumerator GetEnumerator()
		{
			foreach (var dbAccessType in DbAccessTypes)
			{
				//egarLoading, asyncExecution, syncronised
				yield return new object[] { dbAccessType, true, true, false };
				yield return new object[] { dbAccessType, true, false, false };
				yield return new object[] { dbAccessType, true, false, true };
				yield return new object[] { dbAccessType, false, true, false };
				yield return new object[] { dbAccessType, false, false, false };
				yield return new object[] { dbAccessType, false, false, true };
			}
		}
	}

	/// <summary>
	/// Excludes the method from been used by all DbAccessTypes not given in the constructor
	/// </summary>
	/// <seealso cref="NUnit.Framework.NUnitAttribute" />
	/// <seealso cref="NUnit.Framework.Interfaces.IApplyToTest" />
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DbCategoryAttribute : NUnitAttribute, ITestAction
	{
		public DbCategoryAttribute(params DbAccessType[] dbAccessTypes)
		{
			DbAccessTypes = dbAccessTypes;
		}

		public DbAccessType[] DbAccessTypes { get; private set; }

		public void BeforeTest(ITest test)
		{
			var databaseStandardTest = test.Fixture as DatabaseStandardTest;
			if (DbAccessTypes.Any(e => !e.Equals(databaseStandardTest.Type)))
			{
				Assert.Ignore("Test Excluded because opt-out Database-Category");
			}
		}

		public void AfterTest(ITest test)
		{
		}

		public ActionTargets Targets { get; } = ActionTargets.Test;
	}
}


/*
 * <inheritdoc />
		public void ApplyToContext(TestExecutionContext context)
		{
			if (DbAccessTypes.Any(e => !e.Equals((context.CurrentTest as TestFixture ?? context.CurrentTest.Parent as TestFixture)?.Arguments.FirstOrDefault())))
			{
				context.CurrentTest.RunState = RunState.Skipped;
				context.CurrentTest.Properties.Set("_SKIPREASON", "Test Excluded because opt-out Database-Category");
			}
		}

		private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();
		private object _expectedResult;

		/// <summary>Descriptive text for this test</summary>
		public string Description { get; set; }

		/// <summary>The author of this test</summary>
		public string Author { get; set; }

		/// <summary>The type that this test is testing</summary>
		public Type TestOf { get; set; }

		/// <summary>
		/// Modifies a test by adding a description, if not already set.
		/// </summary>
		/// <param name="test">The test to modify</param>
		public void ApplyToTest(Test test)
		{
			if (!test.Properties.ContainsKey("Description") && this.Description != null)
				test.Properties.Set("Description", (object)this.Description);
			if (!test.Properties.ContainsKey("Author") && this.Author != null)
				test.Properties.Set("Author", (object)this.Author);
			if (test.Properties.ContainsKey("TestOf") || !(this.TestOf != (Type)null))
				return;
			test.Properties.Set("TestOf", (object)this.TestOf.FullName);
		}

		/// <summary>Gets or sets the expected result.</summary>
		/// <value>The result.</value>
		public object ExpectedResult
		{
			get
			{
				return this._expectedResult;
			}
			set
			{
				this._expectedResult = value;
				this.HasExpectedResult = true;
			}
		}

		/// <summary>Returns true if an expected result has been set</summary>
		public bool HasExpectedResult { get; private set; }

		/// <summary>Construct a TestMethod from a given method.</summary>
		/// <param name="method">The method for which a test is to be constructed.</param>
		/// <param name="suite">The suite to which the test will be added.</param>
		/// <returns>A TestMethod</returns>
		public TestMethod BuildFromBase(IMethodInfo method, Test suite)
		{
			TestCaseParameters parms = (TestCaseParameters)null;
			if (this.HasExpectedResult)
			{
				parms = new TestCaseParameters();
				parms.ExpectedResult = this.ExpectedResult;
			}
			return this._builder.BuildTestMethod(method, suite, parms);
		}

		public TestMethod BuildFrom(IMethodInfo method, Test suite)
		{
			if (DbAccessTypes.Any(e => !e.Equals((suite as TestFixture ?? suite.Parent as TestFixture)?.Arguments.FirstOrDefault())))
			{
				suite.RunState = RunState.Skipped;
				suite.Properties.Set("_SKIPREASON", "Test Excluded because opt-out Database-Category");
				return null;
			}

			return BuildFromBase(method, suite);
		}
 *
 *
 */
