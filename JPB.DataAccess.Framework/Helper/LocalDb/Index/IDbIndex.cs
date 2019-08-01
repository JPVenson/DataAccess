using System.Collections.Generic;

namespace JPB.DataAccess.Framework.Helper.LocalDb.Index
{
	/// <summary>
	///		Defines an Index(WIP)
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IDbIndex<TEntity> : IEnumerable<TEntity>
	{
		/// <summary>
		///		Name of the Index
		/// </summary>
		string Name { get; }

		/// <summary>
		///		Adds an Entity to the Index
		/// </summary>
		/// <param name="item"></param>
		void Add(TEntity item);
		/// <summary>
		///		Deletes an Entity from the Index
		/// </summary>
		/// <param name="item"></param>
		void Delete(TEntity item);

		/// <summary>
		///		Updates an Entity that exists in the Index
		/// </summary>
		/// <param name="item"></param>
		void Update(TEntity item);
	}
}