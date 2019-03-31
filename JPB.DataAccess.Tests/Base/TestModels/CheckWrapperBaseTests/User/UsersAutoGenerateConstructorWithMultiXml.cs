#region

using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[AutoGenerateCtor]
	[ForModel(UsersMeta.TableName)]
	public sealed class UsersAutoGenerateConstructorWithMultiXml
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }

		[FromXml("Subs", LoadStrategy = LoadStrategy.NotIncludeInSelect)]
		[InsertIgnore]
		public IEnumerable<UsersAutoGenerateConstructorWithSingleXml> Subs { get; set; }
	}
}