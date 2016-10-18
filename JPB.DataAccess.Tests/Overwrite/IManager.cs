using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests
{
	public interface IManager
	{
		DbAccessLayer GetWrapper(DbAccessType type);
		DbAccessType DbAccessType { get; }

		string ConnectionString { get; }
		void FlushErrorData();
	}
}