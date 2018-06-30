#region

using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel(UsersMeta.TableName)]
	public class Users_StaticQueryFactoryForSelectWithArugments
	{
		[PrimaryKey]
		[ForModel(UsersMeta.PrimaryKeyName)]
		public long UserId { get; set; }

		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryBuilder GetSelectStatement(RootQuery builder, long whereId)
		{
			return builder.Select.Table<Users_StaticQueryFactoryForSelectWithArugments>()
				.Where
				.Column(s => s.UserId)
				.Is.EqualsTo(whereId);
		}
	}
}