using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
    public class Users
    {
        [PrimaryKey]
        public long User_ID { get; set; }

        public string UserName { get; set; }
    }

    [AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
    [ForModel(UsersMeta.TableName)]
    public class Users_UsernameAnonymous
    {
        [PrimaryKey]
        public long User_ID { get; set; }

        [AnonymousObjectGeneration]
        public string UserName { get; set; }
    }
}