using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface ISequentialTriggerCollection<TEntity>
	{
		/// <summary>
		/// Will be invoked when the Add function is triggerd.
		/// </summary>
		event EventHandler<ISequentialToken<TEntity>> Insert;

		/// <summary>
		/// Will be invoked when an Entity is updated
		/// </summary>
		event EventHandler<ISequentialToken<TEntity>> Update;

		/// <summary>
		/// Will be invoked when the Remove function is called
		/// </summary>
		event EventHandler<ISequentialToken<TEntity>> Delete;

		/// <summary>
		/// Called when [insert] is invoked.
		/// </summary>
		/// <param name="obj">The object.</param>
		void OnInsert(TEntity obj);
		/// <summary>
		/// Called when [update] is invoked.
		/// </summary>
		/// <param name="obj">The object.</param>
		void OnUpdate(TEntity obj);
		/// <summary>
		/// Called when [delete] is invoked.
		/// </summary>
		/// <param name="obj">The object.</param>
		void OnDelete(TEntity obj);
	}
}