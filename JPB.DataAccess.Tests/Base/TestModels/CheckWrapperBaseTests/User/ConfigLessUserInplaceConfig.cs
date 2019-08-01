#region

using JPB.DataAccess.Framework.DbInfoConfig;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	public class ConfigLessUserInplaceConfig
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }

		[ConfigMehtod]
		public static void Config(DbConfig configBase)
		{
			configBase.SetConfig<ConfigLessUserInplaceConfig>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName);
				f.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName);
			});
		}
	}
}