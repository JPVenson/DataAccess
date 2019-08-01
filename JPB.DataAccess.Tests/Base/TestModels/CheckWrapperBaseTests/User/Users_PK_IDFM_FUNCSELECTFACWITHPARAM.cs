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
	[SelectFactory(UsersMeta.SelectStatement)]
	public class Users_PK_IDFM_FUNCSELECTFACWITHPARAM
	{
		[PrimaryKey]
		[ForModel(UsersMeta.PrimaryKeyName)]
		public long UserId { get; set; }

		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult GetSelectStatement(long whereID)
		{
			return
				new QueryFactoryResult(UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", whereID));
		}
	}
}