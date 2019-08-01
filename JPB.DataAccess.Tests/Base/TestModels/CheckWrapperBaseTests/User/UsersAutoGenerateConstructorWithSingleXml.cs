#region

using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[AutoGenerateCtor]
	[ForModel(UsersMeta.TableName)]
	public sealed class UsersAutoGenerateConstructorWithSingleXml
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }

		[FromXml("Sub", LoadStrategy = LoadStrategy.NotIncludeInSelect)]
		[InsertIgnore]
		public UsersAutoGenerateConstructorWithSingleXml Sub { get; set; }
	}
}