using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [ForModel(UsersMeta.TableName)]
    public class Users_PK
    {
        [PrimaryKey]
        public long User_ID { get; set; }

        public string UserName { get; set; }
    }
}