using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [ForModel(UsersMeta.TableName)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_FUNCSELECT
    {
        [PrimaryKey]
        [ForModel(UsersMeta.PrimaryKeyName)]
        public long UserId { get; set; }

        public string UserName { get; set; }

        [SelectFactoryMethod]
        public static string GetSelectStatement()
        {
            return UsersMeta.SelectStatement;
        }
    }
}