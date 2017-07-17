#region

using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
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