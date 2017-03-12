using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests
{
    public interface IManager
    {
        DbAccessType DbAccessType { get; }

        string ConnectionString { get; }
        DbAccessLayer GetWrapper(DbAccessType type);
        void FlushErrorData();
        void Clear();
    }
}