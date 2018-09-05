using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Query;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite.Misc
{
	[TestFixture]
	public class DataConverterTests
	{
		[Test]
		[TestCase("1", typeof(int), 1)]
		[TestCase(nameof(EnumerationMode.FullOnLoad), typeof(EnumerationMode), EnumerationMode.FullOnLoad)]
		[TestCase(1, typeof(bool), true)]
		[TestCase(0, typeof(bool), false)]
		
		[TestCase("1", typeof(bool), true)]
		[TestCase("0", typeof(bool), false)]

		//[TestCase("12/31/99 23:59:59 +00:00", typeof(DateTimeOffset), DateTimeOffset.MaxValue)]
		public void TestChangeType(object input, Type toType, object expected)
		{
			Assert.That(() => DataConverterExtensions.ChangeType(input, toType), Is.EqualTo(expected));
		}
		
		[Test]
		public void TestChangeDateType()
		{
			Assert.That(() => DataConverterExtensions.ChangeType("9999-12-31 23:59:59.9999999+00:00", typeof(DateTimeOffset)), Is.EqualTo(DateTimeOffset.MaxValue));
		}
	}
}
