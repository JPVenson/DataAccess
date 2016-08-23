using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
    public interface IConstraintCollection<TEntity>
    {
        ICheckConstraints<TEntity> Check { get; }
        IDefaultConstraints<TEntity> Default { get; }
        ILocalDbPrimaryKeyConstraint PrimaryKey { get; }
        IUniqueConstrains<TEntity> Unique { get; }
    }
}