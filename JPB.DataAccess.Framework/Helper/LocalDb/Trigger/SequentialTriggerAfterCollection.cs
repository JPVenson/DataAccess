namespace JPB.DataAccess.Framework.Helper.LocalDb.Trigger
{
	//public class SequentialTriggerAfterCollection<TToken, TEntity>
	//	: ISequentialTriggerAfterCollection<,> 
	//	where TToken : ISequentialToken<TEntity>
	//{
	//	protected readonly LocalDbReposetory<TEntity> Tabel;
	//	protected readonly ISequentialTriggerCollection<TToken, TEntity> Duplication;

	//	protected virtual event EventHandler<TToken> _insert;
	//	protected virtual event EventHandler<TToken> _update;
	//	protected virtual event EventHandler<TToken> _delete;

	//	internal SequentialTriggerAfterCollection(LocalDbReposetory<TEntity> tabel, ISequentialTriggerCollection<TToken, TEntity> duplication = null)
	//	{
	//		Tabel = tabel;
	//		Duplication = duplication;
	//	}

	//	/// <summary>
	//	/// Will be invoked when the Add function is triggerd.
	//	/// </summary>
	//	public virtual event EventHandler<TToken> Insert
	//	{
	//		add
	//		{
	//			_insert += value;
	//			if (Duplication != null)
	//				Duplication.Insert += value;
	//		}
	//		remove
	//		{
	//			_insert -= value;
	//			if (Duplication != null)
	//				Duplication.Insert -= value;
	//		}
	//	}

	//	/// <summary>
	//	/// Will be invoked when an Entity is updated
	//	/// </summary>
	//	public virtual event EventHandler<TToken> Update
	//	{
	//		add
	//		{
	//			_update += value;
	//			if (Duplication != null)
	//				Duplication.Update += value;
	//		}
	//		remove
	//		{
	//			_update -= value;
	//			if (Duplication != null)
	//				Duplication.Update -= value;
	//		}
	//	}

	//	/// <summary>
	//	/// Will be invoked when the Remove function is called
	//	/// </summary>
	//	public virtual event EventHandler<TToken> Delete
	//	{
	//		add
	//		{
	//			_delete += value;
	//			if (Duplication != null)
	//				Duplication.Delete += value;
	//		}
	//		remove
	//		{
	//			_delete -= value;
	//			if (Duplication != null)
	//				Duplication.Delete -= value;
	//		}
	//	}

	//	protected abstract void InvokeTrigger(EventHandler<TToken> trigger, TEntity obj);

	//	public virtual void OnInsert(TEntity obj)
	//	{
	//		InvokeTrigger(_insert, obj);
	//	}

	//	public virtual void OnUpdate(TEntity obj)
	//	{
	//		InvokeTrigger(_update, obj);
	//	}

	//	public virtual void OnDelete(TEntity obj)
	//	{
	//		InvokeTrigger(_delete, obj);
	//	}
	//}
}