using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace JPB.DataAccess.Tests.TestFramework
{
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
				test.Properties.Set("_SKIPREASON", "Test Excluded because opt-out");
			}
		}
	}
}
