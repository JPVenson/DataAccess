using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Contracts
{
	public interface ILocalDbDefaultConstraint<in TEntity> : ILocalDbConstraint
	{
		void DefaultValue(TEntity item);
	}
}