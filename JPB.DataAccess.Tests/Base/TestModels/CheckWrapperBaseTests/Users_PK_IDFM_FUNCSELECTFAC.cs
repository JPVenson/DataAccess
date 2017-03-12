using JPB.DataAccess.Contacts;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [ForModel(UsersMeta.TableName)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_FUNCSELECTFAC
    {
        [PrimaryKey]
        [ForModel(UsersMeta.PrimaryKeyName)]
        public long UserId { get; set; }

        public string UserName { get; set; }

        [SelectFactoryMethod]
        public static IQueryFactoryResult GetSelectStatement()
        {
            return new QueryFactoryResult(UsersMeta.SelectStatement);
        }
    }
}