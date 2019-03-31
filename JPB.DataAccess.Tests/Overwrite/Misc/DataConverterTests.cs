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

		[TestCase(true, typeof(string), "True")]
		[TestCase(false, typeof(string), "False")]

		//[TestCase("12/31/99 23:59:59 +00:00", typeof(DateTimeOffset), DateTimeOffset.MaxValue)]
		public void TestChangeType(object input, Type toType, object expected)
		{
			Assert.That(DataConverterExtensions.ChangeType(ref input, toType), Is.True);
			Assert.That(input, Is.EqualTo(expected));
		}
		
		[Test]
		public void TestChangeDateType()
		{
			object value = "9999-12-31 23:59:59.9999999+00:00";
			Assert.That(DataConverterExtensions.ChangeType(ref value, 
				typeof(DateTimeOffset)), Is.True);
			Assert.That(value, Is.EqualTo(DateTimeOffset.MaxValue));
		}
	}
}
