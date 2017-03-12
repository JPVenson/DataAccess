using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [ForModel(UsersMeta.TableName)]
    public class Users_PK_IDFM
    {
        [PrimaryKey]
        [ForModel(UsersMeta.PrimaryKeyName)]
        public long UserId { get; set; }

        public string UserName { get; set; }
    }
}