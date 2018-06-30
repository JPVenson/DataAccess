using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData
{
	public interface IDatabaseMeta
	{
		string CreationCommand(DbAccessType accessType);
	}
}