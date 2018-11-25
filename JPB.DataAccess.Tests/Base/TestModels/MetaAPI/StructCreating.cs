﻿using System;

namespace JPB.DataAccess.Tests.Base.TestModels.MetaAPI
{
	public struct StructCreating
	{
		public StructCreating(string propString)
		{
			PropString = propString;
		}

		public string PropString { get; private set; }
	}

	public class NestedClassTest
	{
		public string PropString { get; set; }

		class PrivateNestedClass
		{
			public int PropInt { get; set; }
		}

		public static Type GetNestedClassType()
		{
			return typeof(PrivateNestedClass);
		}
	}

	public class ClassCreating
	{
	}

	public class ClassCreatingWithArguments
	{
		public ClassCreatingWithArguments(string propString)
		{
			PropString = propString;
		}

		public string PropString { get; set; }
	}

	public class ClassSpeedMeasurement
	{
		public string PropString { get; set; }
	}
}