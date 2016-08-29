using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	//public class InsteadtOfActionToken : EventArgs
	//{
	//	public InsteadtOfActionToken(object item, LocalDbReposetoryBase table)
	//	{
	//		Item = item;
	//		Table = table;
	//	}

	//	public virtual object Item { get; private set; }
	//	public LocalDbReposetoryBase Table { get; private set; }
	//}

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.IInsteadtOfActionToken{TEntity}" />
	public class InsteadtOfActionToken<TEntity> : IInsteadtOfActionToken<TEntity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InsteadtOfActionToken{TEntity}"/> class.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="table">The table.</param>
		public InsteadtOfActionToken(TEntity item, LocalDbRepository<TEntity> table)
		{
			Item = item;
			Table = table;
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <value>
		/// The item.
		/// </value>
		public virtual TEntity Item { get; private set; }
		/// <summary>
		/// Gets the table.
		/// </summary>
		/// <value>
		/// The table.
		/// </value>
		public LocalDbRepository<TEntity> Table { get; private set; }
	}
}