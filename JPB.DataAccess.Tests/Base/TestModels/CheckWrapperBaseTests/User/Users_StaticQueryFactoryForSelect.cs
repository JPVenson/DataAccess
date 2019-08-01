#region

using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Framework.QueryFactory;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[ForModel(UsersMeta.TableName)]
	public class Users_StaticQueryFactoryForSelect
	{
		[PrimaryKey]
		[ForModel(UsersMeta.PrimaryKeyName)]
		public long UserId { get; set; }

		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult GetSelectStatement()
		{
			return new QueryFactoryResult($"SELECT * FROM {UsersMeta.TableName}");
		}
	}
}