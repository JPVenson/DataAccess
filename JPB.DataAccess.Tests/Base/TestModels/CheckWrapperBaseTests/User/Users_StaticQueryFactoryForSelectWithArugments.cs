#region

using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Framework.QueryFactory;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[ForModel(UsersMeta.TableName)]
	public class Users_StaticQueryFactoryForSelectWithArugments
	{
		[PrimaryKey]
		[ForModel(UsersMeta.PrimaryKeyName)]
		public long UserId { get; set; }

		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult GetSelectStatement(long whereId)
		{
			return new QueryFactoryResult($"SELECT * FROM {UsersMeta.TableName} WHERE {UsersMeta.PrimaryKeyName} = @arg", 
				new QueryParameter("arg", whereId));
		}
	}
}