using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface ITriggerInsteadtOfCollection<TEntity>
	{
		event EventHandler<IInsteadtOfActionToken<TEntity>> Insert;
		event EventHandler<IInsteadtOfActionToken<TEntity>> Update;
		event EventHandler<IInsteadtOfActionToken<TEntity>> Delete;
		bool OnInsert(TEntity obj);
		bool OnUpdate(TEntity obj);
		bool OnDelete(TEntity obj);
	}

	public class TriggerInsteadtOfCollection<TEntity> : ITriggerInsteadtOfCollection<TEntity>
	{
		private readonly LocalDbRepository<TEntity> _tabel;
		private readonly ITriggerInsteadtOfCollection<TEntity> _duplication;

		internal TriggerInsteadtOfCollection(LocalDbRepository<TEntity> tabel, ITriggerInsteadtOfCollection<TEntity> duplication = null)
		{
			_tabel = tabel;
			_duplication = duplication;
		}

		internal TriggerInsteadtOfCollection()
		{
		}

		private event EventHandler<IInsteadtOfActionToken<TEntity>> _insert;
		private event EventHandler<IInsteadtOfActionToken<TEntity>> _update;
		private event EventHandler<IInsteadtOfActionToken<TEntity>> _delete;

		public virtual event EventHandler<IInsteadtOfActionToken<TEntity>> Insert
		{
			add
			{
				_insert += value;
				if (_duplication != null)
					_duplication.Insert += value;
			}
			remove
			{
				_insert -= value;
				if (_duplication != null)
					_duplication.Insert -= value;
			}
		}

		/// <summary>
		/// Will be invoked when an Entity is updated
		/// </summary>
		public virtual event EventHandler<IInsteadtOfActionToken<TEntity>> Update
		{
			add
			{
				_update += value;
				if (_duplication != null)
					_duplication.Update += value;
			}
			remove
			{
				_update -= value;
				if (_duplication != null)
					_duplication.Update -= value;
			}
		}

		/// <summary>
		/// Will be invoked when the Remove function is called
		/// </summary>
		public virtual event EventHandler<IInsteadtOfActionToken<TEntity>> Delete
		{
			add
			{
				_delete += value;
				if (_duplication != null)
					_duplication.Delete += value;
			}
			remove
			{
				_delete -= value;
				if (_duplication != null)
					_duplication.Delete -= value;
			}
		}

		[ThreadStatic]
		internal static bool AsInsteadtOf;

		private bool InvokeTrigger(EventHandler<IInsteadtOfActionToken<TEntity>> trigger, TEntity obj)
		{
			if (_tabel == null)
				return false;

			var token = new InsteadtOfActionToken<TEntity>(obj, _tabel);
			if (trigger != null)
			{
				try
				{
					AsInsteadtOf = true;
					trigger(this, token);
				}
				finally
				{
					AsInsteadtOf = false;
				}
				return true;
			}
			return false;
		}

		public virtual bool OnInsert(TEntity obj)
		{
			return InvokeTrigger(_insert, obj);
		}

		public virtual bool OnUpdate(TEntity obj)
		{
			return InvokeTrigger(_update, obj);
		}

		public virtual bool OnDelete(TEntity obj)
		{
			return InvokeTrigger(_delete, obj);
		}

		public static TriggerInsteadtOfCollection<TEntity> Empty()
		{
			return new TriggerInsteadtOfCollection<TEntity>();
		}
	}
}