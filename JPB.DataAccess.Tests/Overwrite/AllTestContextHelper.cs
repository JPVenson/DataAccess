#region

using JPB.DataAccess.DbInfoConfig;

#endregion

namespace JPB.DataAccess.Tests.Overwrite
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