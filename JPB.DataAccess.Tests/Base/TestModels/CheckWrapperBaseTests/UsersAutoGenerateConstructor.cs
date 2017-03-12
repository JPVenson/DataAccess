using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [AutoGenerateCtor]
    [ForModel(UsersMeta.TableName)]
    public sealed class UsersAutoGenerateConstructor
    {
        [PrimaryKey]
        public long User_ID { get; set; }

        public string UserName { get; set; }
    }
}