using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [AutoGenerateCtor]
    [ForModel(UsersMeta.TableName)]
    public class UsersAutoGenerateNullableConstructor
    {
        [PrimaryKey]
        public long? User_ID { get; set; }

        public string UserName { get; set; }
    }
}