using System.Collections.Generic;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public interface ICheckConstraints<TEntity> : ICollection<ILocalDbCheckConstraint<TEntity>>
	{
		void Enforce(TEntity item);
	}
}