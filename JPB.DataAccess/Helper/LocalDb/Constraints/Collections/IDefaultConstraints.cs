using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public interface IDefaultConstraints<TEntity> : ICollection<ILocalDbDefaultConstraint<TEntity>>
	{
		void Enforce(TEntity elementToAdd);
	}
}