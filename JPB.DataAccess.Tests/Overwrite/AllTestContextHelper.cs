#region

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

		public static void TearDown(this BaseTest that)
		{
			if (Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Failure) ||
			    Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Error))
			{
				that.Mgr.FlushErrorData();
			}
			else
			{
				that.Clear();
			}
		}

		public static void ClearDb(this BaseTest that)
		{
			if (that.DbAccess != null)
			{
				that.DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
				if (that.DbAccess.DbAccessType == DbAccessType.MsSql)
				{
					that.DbAccess.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
				}

				if (that.DbAccess.Config != null)
				{
					that.DbAccess.Config.Dispose();
				}
			}
		}

		public static void DeleteDb(this BaseTest that)
		{
			var redesginDatabase = string.Format("IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
				that.DbAccess.Database.DatabaseName);
			that.DbAccess.ExecuteGenericCommand(that.DbAccess.Database.CreateCommand(redesginDatabase));
		}
	}
}