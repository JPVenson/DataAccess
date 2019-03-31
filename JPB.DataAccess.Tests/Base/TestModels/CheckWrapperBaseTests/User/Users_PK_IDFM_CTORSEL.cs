#region

using System.Data;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel(UsersMeta.TableName)]
	public class Users_PK_IDFM_CTORSEL
	{
		public Users_PK_IDFM_CTORSEL(IDataRecord rec)
		{
			UserName = (string) rec[UsersMeta.ContentName];
			UserId = (long) rec[UsersMeta.PrimaryKeyName];
		}

		[PrimaryKey]
		[ForModel(UsersMeta.PrimaryKeyName)]
		public long UserId { get; set; }

		public string UserName { get; set; }
	}
}