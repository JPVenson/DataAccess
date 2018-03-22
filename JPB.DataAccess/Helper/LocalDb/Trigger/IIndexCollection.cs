using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Index;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	///		Combines multible Indexes
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IIndexCollection<TEntity> : IEnumerable<IDbIndex<TEntity>>
	{

	}
}