using System.Collections.Generic;
using System.Linq;

namespace JPB.DataAccess.Helper.LocalDb.Index
{
	public interface IDbIndex<TEntity> : IEnumerable<TEntity>
	{
		string Name { get; }

		void Add(TEntity item);
		void Delete(TEntity item);
		void Update(TEntity item);
	}
}