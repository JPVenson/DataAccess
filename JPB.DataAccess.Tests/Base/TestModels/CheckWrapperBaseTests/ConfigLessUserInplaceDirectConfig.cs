#region

using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public class ConfigLessUserInplaceDirectConfig
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }

		[ConfigMehtod]
		public static void Config(ConfigurationResolver<ConfigLessUserInplaceDirectConfig> configBase)
		{
			configBase.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
			configBase.SetPrimaryKey(e => e.PropertyA);
			configBase.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName);
			configBase.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName);
		}
	}
}