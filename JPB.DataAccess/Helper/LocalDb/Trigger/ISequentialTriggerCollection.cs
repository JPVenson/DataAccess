using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
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

		void OnInsert(TEntity obj);
		void OnUpdate(TEntity obj);
		void OnDelete(TEntity obj);
	}
}