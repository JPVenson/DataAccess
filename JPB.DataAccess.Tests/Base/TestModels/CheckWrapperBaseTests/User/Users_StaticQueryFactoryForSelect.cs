#region

using JPB.DataAccess.Contacts;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

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
		public static IQueryFactoryResult GetSelectStatement()
		{
			return new QueryFactoryResult($"SELECT * FROM {UsersMeta.TableName}");
		}
	}
}