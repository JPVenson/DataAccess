#region

using System;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.DbAccessLayerTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

#endregion

namespace JPB.DataAccess.Tests
{
	public static class AllTestContextHelper
	{
		public static void TestSetup(FactoryHelperSettings config = null)
		{
			FactoryHelperSettings targetSetting;
			if (config == null)
			{
				targetSetting = FactoryHelperSettings.DefaultSettings;
			}
			else
			{
				targetSetting = config;
			}

			targetSetting.CreateDebugCode = false;
			targetSetting.EnforceCreation = true;
			targetSetting.FileCollisonDetection = CollisonDetectionMode.Pessimistic;
		}
	}
}