using System;
using System.Linq;
using JPB.DataAccess.Framework.Manager;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace JPB.DataAccess.Tests.TestFramework
{
	/// <summary>
	/// Excludes the method from been used by all DbAccessTypes not given in the constructor
	/// </summary>
	/// <seealso cref="NUnit.Framework.NUnitAttribute" />
	/// <seealso cref="NUnit.Framework.Interfaces.IApplyToTest" />
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DbCategoryAttribute : NUnitAttribute, IApplyToTest
	{
		public DbCategoryAttribute(params DbAccessType[] dbAccessTypes)
		{
			DbAccessTypes = dbAccessTypes;
		}

		public DbAccessType[] DbAccessTypes { get; private set; }

		///// <inheritdoc />
		//public void ApplyToContext(TestExecutionContext context)
		//{
		//	if (DbAccessTypes.Any(e => !e.Equals((context.CurrentTest.Parent as TestFixture)?.Arguments.FirstOrDefault())))
		//	{
		//		context.CurrentTest.RunState = RunState.NotRunnable;
		//		context.CurrentTest.Properties.Set("_SKIPREASON", "Test Excluded because opt-out");
		//	}
		//}

		/// <inheritdoc />
		public void ApplyToTest(Test test)
		{
			if (DbAccessTypes.Any(e => !e.Equals((test.Parent as TestFixture)?.Arguments.FirstOrDefault())))
			{
				test.RunState = RunState.Skipped;
				test.Properties.Set("_SKIPREASON", "Test Excluded because opt-out Category");
			}
		}
	}
}
