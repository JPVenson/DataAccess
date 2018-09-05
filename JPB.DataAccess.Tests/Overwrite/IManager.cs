#region

using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.Tests.Overwrite
{
	public interface IManager
	{
		DbAccessLayer GetWrapper(DbAccessType type, params object[] additionalArguments);
		void FlushErrorData();
		void Clear();
	}


	public interface IManagerImplementation
	{
		DbAccessType DbAccessType { get; }
		string ConnectionString { get; }
		DbAccessLayer GetWrapper(DbAccessType type, string instanceName);
		void FlushErrorData();
		void Clear();
	}
}