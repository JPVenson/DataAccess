using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Index;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface IIndexCollection<TEntity> : IEnumerable<IDbIndex<TEntity>>
	{

	}
}