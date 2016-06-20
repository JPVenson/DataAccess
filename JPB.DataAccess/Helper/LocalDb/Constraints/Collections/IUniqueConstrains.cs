using System;
using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public interface IUniqueConstrains<TEntity> : ICollection<ILocalDbUniqueConstraint<TEntity>>
	{
		void Add<TValue>(string name, Func<TEntity, TValue> item);
		void Enforce(TEntity item);
		void ItemAdded(TEntity item);
		void ItemRemoved(TEntity item);
		void ItemUpdated(TEntity item);
	}
}