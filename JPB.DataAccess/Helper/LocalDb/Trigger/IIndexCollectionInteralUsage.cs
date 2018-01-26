using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Index;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	internal interface IIndexCollectionInteralUsage<TEntity> : IIndexCollection<TEntity>, ICollection<IDbIndex<TEntity>>
	{
		void Add(TEntity item);
		void Delete(TEntity item);
		void Update(TEntity item);
	}
}