using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Contracts
{
	public interface ILocalDbUniqueConstraint<in TEntity> : ILocalDbCheckConstraint<TEntity>
	{
		void Add(TEntity item);
		void Delete(TEntity item);
		void Update(TEntity item);
	}
}