using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.ISequentialTriggerCollection{TEntity}" />
	public class SequentialSequentialTriggerCollection<TEntity>
		: ISequentialTriggerCollection<TEntity>
	{
		protected readonly LocalDbRepository<TEntity> Tabel;
		/// <summary>
		/// The Collection that should be mirrored
		/// </summary>
		protected readonly ISequentialTriggerCollection<TEntity> Duplication;

		private event EventHandler<ISequentialToken<TEntity>> _insert;
		private event EventHandler<ISequentialToken<TEntity>> _update;
		private event EventHandler<ISequentialToken<TEntity>> _delete;

		internal SequentialSequentialTriggerCollection(LocalDbRepository<TEntity> tabel, ISequentialTriggerCollection<TEntity> duplication = null)
		{
			Tabel = tabel;
			Duplication = duplication;
		}

		/// <summary>
		/// Will be invoked when the Add function is triggerd.
		/// </summary>
		public virtual event EventHandler<ISequentialToken<TEntity>> Insert
		{
			add
			{
				_insert += value;
				if (Duplication != null)
					Duplication.Insert += value;
			}
			remove
			{
				_insert -= value;
				if (Duplication != null)
					Duplication.Insert -= value;
			}
		}

		/// <summary>
		/// Will be invoked when an Entity is updated
		/// </summary>
		public virtual event EventHandler<ISequentialToken<TEntity>> Update
		{
			add
			{
				_update += value;
				if (Duplication != null)
					Duplication.Update += value;
			}
			remove
			{
				_update -= value;
				if (Duplication != null)
					Duplication.Update -= value;
			}
		}

		/// <summary>
		/// Will be invoked when the Remove function is called
		/// </summary>
		public virtual event EventHandler<ISequentialToken<TEntity>> Delete
		{
			add
			{
				_delete += value;
				if (Duplication != null)
					Duplication.Delete += value;
			}
			remove
			{
				_delete -= value;
				if (Duplication != null)
					Duplication.Delete -= value;
			}
		}

		protected void InvokeTrigger(EventHandler<ISequentialToken<TEntity>> trigger, TEntity obj)
		{
			var token = new SequentialToken<TEntity>(obj);
			if (trigger != null)
				trigger.Invoke(this, token);
			if (token.Canceled)
				throw new TriggerException<TEntity>(token.Reason, Tabel);
		}

		public virtual void OnInsert(TEntity obj)
		{
			InvokeTrigger(_insert, obj);
		}

		public virtual void OnUpdate(TEntity obj)
		{
			InvokeTrigger(_update, obj);
		}

		public virtual void OnDelete(TEntity obj)
		{
			InvokeTrigger(_delete, obj);
		}
	}

	//public class TriggerForCollection<TEntity>
	//	: TriggerForCollection<IForActionCancelToken<TEntity>, TEntity>
	//{
	//	public TriggerForCollection(LocalDbReposetory<TEntity> tabel, ITriggerForCollection<IForActionCancelToken<TEntity>, TEntity> duplication = null)
	//		: base(tabel, duplication)
	//	{
	//	}

	//	protected override void InvokeTrigger(EventHandler<IForActionCancelToken<TEntity>> trigger, TEntity obj)
	//	{
	//		var token = new ForActionCancelToken<TEntity>(obj);
	//		if (trigger != null)
	//			trigger.Invoke(this, token);
	//		if (token.Canceled)
	//			throw new TriggerException<TEntity>(token.Reason, Tabel);
	//	}
	//}
}