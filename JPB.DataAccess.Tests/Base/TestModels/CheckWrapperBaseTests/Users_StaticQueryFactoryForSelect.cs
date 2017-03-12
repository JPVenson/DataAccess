using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query.Operators;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    [ForModel(UsersMeta.TableName)]
    public class Users_StaticQueryFactoryForSelect
    {
        [PrimaryKey]
        [ForModel(UsersMeta.PrimaryKeyName)]
        public long UserId { get; set; }

        public string UserName { get; set; }

        [SelectFactoryMethod]
        public static void GetSelectStatement(RootQuery builder)
        {
            builder.Select.Table<Users_StaticQueryFactoryForSelect>();
        }
    }
}