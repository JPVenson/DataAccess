#region

using JPB.DataAccess.Contacts;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
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