using System.Collections.Generic;
using JPB.DataAccess.Framework.Helper.LocalDb.Index;

namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	internal interface IIndexCollectionInteralUsage<TEntity> : IIndexCollection<TEntity>, ICollection<IDbIndex<TEntity>>
	{
		void Add(TEntity item);
		void Delete(TEntity item);
		void Update(TEntity item);
	}
}