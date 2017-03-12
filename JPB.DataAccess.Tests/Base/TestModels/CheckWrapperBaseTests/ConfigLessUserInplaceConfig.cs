using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
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