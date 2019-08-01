using System.Collections.Generic;
using JPB.DataAccess.Framework.Helper.LocalDb.Index;

namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	/// <summary>
	///		Combines multible Indexes
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IIndexCollection<TEntity> : IEnumerable<IDbIndex<TEntity>>
	{

	}
}