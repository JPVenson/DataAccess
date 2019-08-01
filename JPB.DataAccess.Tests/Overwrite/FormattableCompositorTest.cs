using System;
using JPB.DataAccess.Framework.Helper;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite
{
	[TestFixture]
	public class FormattableCompositorTest
	{
		public FormattableCompositorTest()
		{

		}

		[Test]
		public void CanFormatSimpleFormat()
		{
			var result = FormattableStringCompositor.Factory($"one: {0}, two, {1}, three {2}");
			Assert.That(result.Query, Is.EqualTo("one: @arg_0, two, @arg_1, three @arg_2"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(3));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo(0));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo(1));
			Assert.That(result.QueryParameters[2].Value, Is.EqualTo(2));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
			Assert.That(result.QueryParameters[2].Name, Is.EqualTo("@arg_2"));
		}

		[Test]
		public void CanFormatOffsetFormat()
		{
			var result = FormattableStringCompositor.Factory($"one: {0,2}, two, {1,3}, three {2,5}");
			Assert.That(result.Query, Is.EqualTo("one: @arg_0, two, @arg_1, three @arg_2"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(3));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo($"{0,2}"));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo($"{1,3}"));
			Assert.That(result.QueryParameters[2].Value, Is.EqualTo($"{2,5}"));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
			Assert.That(result.QueryParameters[2].Name, Is.EqualTo("@arg_2"));
		}

		[Test]
		public void CanFormatOffsetWithSameFormat()
		{
			var result = FormattableStringCompositor.Factory($"one: {0,2}, two, {0,3}, three {2,5}");
			Assert.That(result.Query, Is.EqualTo("one: @arg_0, two, @arg_1, three @arg_2"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(3));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo($"{0,2}"));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo($"{0,3}"));
			Assert.That(result.QueryParameters[2].Value, Is.EqualTo($"{2,5}"));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
			Assert.That(result.QueryParameters[2].Name, Is.EqualTo("@arg_2"));
		}

		[Test]
		public void CanFormatWithInFormatFormat()
		{
			var dateTime = DateTime.Now;
			var result = FormattableStringCompositor.Factory($"one: {0:D5}, two, {0:X}, three {dateTime:D}");
			Assert.That(result.Query, Is.EqualTo("one: @arg_0, two, @arg_1, three @arg_2"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(3));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo($"{0:D5}"));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo($"{0:X}"));
			Assert.That(result.QueryParameters[2].Value, Is.EqualTo($"{dateTime:D}"));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
			Assert.That(result.QueryParameters[2].Name, Is.EqualTo("@arg_2"));
		}

		[Test]
		public void CanFormatWithEscapedFormat()
		{
			var dateTime = DateTime.Now;
			var result = FormattableStringCompositor.Factory($"one: {{0:D5}}, two, {0:X}, three {dateTime:D}");
			Assert.That(result.Query, Is.EqualTo("one: {{0:D5}}, two, @arg_0, three @arg_1"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(2));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo($"{0:X}"));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo($"{dateTime:D}"));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
		}

		[Test]
		public void CanFormatWithMixedEscapedFormat()
		{
			var dateTime = DateTime.Now;
			var result = FormattableStringCompositor.Factory($"one: {{0:D5}}, two, {0:X}, three {dateTime,12}");
			Assert.That(result.Query, Is.EqualTo("one: {{0:D5}}, two, @arg_0, three @arg_1"));
			Assert.That(result.QueryParameters.Length, Is.EqualTo(2));
			Assert.That(result.QueryParameters[0].Value, Is.EqualTo($"{0:X}"));
			Assert.That(result.QueryParameters[1].Value, Is.EqualTo($"{dateTime,12}"));
			Assert.That(result.QueryParameters[0].Name, Is.EqualTo("@arg_0"));
			Assert.That(result.QueryParameters[1].Name, Is.EqualTo("@arg_1"));
		}
	}
}
