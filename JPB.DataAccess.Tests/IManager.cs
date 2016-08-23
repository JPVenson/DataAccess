using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests.Common
{
	public interface IManager
	{
		DbAccessLayer GetWrapper();
		void FlushErrorData();
	}
}