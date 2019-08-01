using System;

namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface ITriggerInsteadtOfCollection<TEntity>
	{
		/// <summary>
		///     Occurs when [insert] is trigged.
		/// </summary>
		event EventHandler<IInsteadtOfActionToken<TEntity>> Insert;

		/// <summary>
		///     Occurs when [update] is trigged.
		/// </summary>
		event EventHandler<IInsteadtOfActionToken<TEntity>> Update;

		/// <summary>
		///     Occurs when [delete] is trigged.
		/// </summary>
		event EventHandler<IInsteadtOfActionToken<TEntity>> Delete;

		/// <summary>
		///     Called when an inserd is called.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>True if the Obj is handeld</returns>
		bool OnInsert(TEntity obj);

		/// <summary>
		///     Called when [update].
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>True if the Obj is handeld</returns>
		bool OnUpdate(TEntity obj);

		/// <summary>
		///     Called when [delete].
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>True if the Obj is handeld</returns>
		bool OnDelete(TEntity obj);
	}
}