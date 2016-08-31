using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	//public class ForActionCancelToken : EventArgs
	//{
	//	public virtual object Item { get; private set; }
	//	public bool Canceled { get; private set; }
	//	public string Reason { get; private set; }

	//	public ForActionCancelToken(object item)
	//	{
	//		Item = item;
	//	}

	//	public void Cancel(string reason)
	//	{
	//		this.Reason = reason;
	//		this.Canceled = true;
	//	}
	//}

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.ISequentialToken{TEntity}" />
	public class SequentialToken<TEntity> : ISequentialToken<TEntity>
	{
		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <value>
		/// The item.
		/// </value>
		public TEntity Item { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:JPB.DataAccess.Helper.LocalDb.Trigger.ISequentialToken`1" /> is canceled.
		/// </summary>
		/// <value>
		/// <c>true</c> if canceled; otherwise, <c>false</c>.
		/// </value>
		public bool Canceled { get; private set; }
		/// <summary>
		/// Gets the reason.
		/// </summary>
		/// <value>
		/// The reason.
		/// </value>
		public string Reason { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SequentialToken{TEntity}"/> class.
		/// </summary>
		/// <param name="item">The item.</param>
		public SequentialToken(TEntity item)
		{
			Item = item;
		}

		/// <summary>
		/// Cancels with the specified reason.
		/// </summary>
		/// <param name="reason">The reason.</param>
		public void Cancel(string reason)
		{
			this.Reason = reason;
			this.Canceled = true;
		}
	}
}