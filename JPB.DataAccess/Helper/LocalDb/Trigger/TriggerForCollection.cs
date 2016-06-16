using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerForCollection
	{
		private readonly LocalDbReposetoryBase _tabel;
		private readonly TriggerForCollection _duplication;

		private event EventHandler<ForActionCancelToken> _insert;
		private event EventHandler<ForActionCancelToken> _update;
		private event EventHandler<ForActionCancelToken> _delete;

		internal TriggerForCollection(LocalDbReposetoryBase tabel, TriggerForCollection duplication = null)
		{
			_tabel = tabel;
			_duplication = duplication;
		}

		/// <summary>
		/// Will be invoked when the Add function is triggerd.
		/// </summary>
		public event EventHandler<ForActionCancelToken> Insert
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
		public event EventHandler<ForActionCancelToken> Update
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
		public event EventHandler<ForActionCancelToken> Delete
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

		private void InvokeTrigger(EventHandler<ForActionCancelToken> trigger, object obj)
		{
			var token = new ForActionCancelToken(obj);
			if (trigger != null)
				trigger.Invoke(this, token);
			if (token.Canceled)
				throw new TriggerException(token.Reason, _tabel);
		}

		internal virtual void OnInsert(object obj)
		{
			InvokeTrigger(_insert, obj);
		}

		internal virtual void OnUpdate(object obj)
		{
			InvokeTrigger(_update, obj);
		}

		internal virtual void OnDelete(object obj)
		{
			InvokeTrigger(_delete, obj);
		}
	}
}