using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace JPB.DataAccess.Tests.TestFramework
{
	public class StandardDatabaseTests : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			yield return new object[] {DbAccessType.MsSql, true, false };
			yield return new object[] {DbAccessType.MsSql, false, false };
			yield return new object[] {DbAccessType.MsSql, false, true };

			yield return new object[] {DbAccessType.SqLite, true, false };
			yield return new object[] {DbAccessType.SqLite, false, false };
			yield return new object[] {DbAccessType.SqLite, false, true };

			yield return new object[] {DbAccessType.MySql, true, false };
			yield return new object[] {DbAccessType.MySql, false, false };
			yield return new object[] {DbAccessType.MySql, false, true };
		}
	}

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
