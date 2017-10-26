#region

using System;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Trigger
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

	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.ITriggerInsteadtOfCollection{TEntity}" />
	public class TriggerInsteadtOfCollection<TEntity> : ITriggerInsteadtOfCollection<TEntity>
	{
		/// <summary>
		///     Used to allow chained actions.
		/// </summary>
		[ThreadStatic] internal static bool AsInsteadtOf;

		private readonly ITriggerInsteadtOfCollection<TEntity> _duplication;
		private readonly LocalDbRepository<TEntity> _tabel;

		internal TriggerInsteadtOfCollection(LocalDbRepository<TEntity> tabel,
			ITriggerInsteadtOfCollection<TEntity> duplication = null)
		{
			_tabel = tabel;
			_duplication = duplication;
		}

		internal TriggerInsteadtOfCollection()
		{
		}

		/// <summary>
		///     Occurs when [insert] is trigged.
		/// </summary>
		public event EventHandler<IInsteadtOfActionToken<TEntity>> Insert
		{
			add
			{
				_insert += value;
				if (_duplication != null)
				{
					_duplication.Insert += value;
				}
			}
			remove
			{
				_insert -= value;
				if (_duplication != null)
				{
					_duplication.Insert -= value;
				}
			}
		}

		/// <summary>
		///     Will be invoked when an Entity is updated
		/// </summary>
		public event EventHandler<IInsteadtOfActionToken<TEntity>> Update
		{
			add
			{
				_update += value;
				if (_duplication != null)
				{
					_duplication.Update += value;
				}
			}
			remove
			{
				_update -= value;
				if (_duplication != null)
				{
					_duplication.Update -= value;
				}
			}
		}

		/// <summary>
		///     Will be invoked when the Remove function is called
		/// </summary>
		public event EventHandler<IInsteadtOfActionToken<TEntity>> Delete
		{
			add
			{
				_delete += value;
				if (_duplication != null)
				{
					_duplication.Delete += value;
				}
			}
			remove
			{
				_delete -= value;
				if (_duplication != null)
				{
					_duplication.Delete -= value;
				}
			}
		}

		/// <summary>
		///     Called when an inserd is called.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     True if the Obj is handeld
		/// </returns>
		public bool OnInsert(TEntity obj)
		{
			return InvokeTrigger(_insert, obj);
		}

		/// <summary>
		///     Called when [update].
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     True if the Obj is handeld
		/// </returns>
		public bool OnUpdate(TEntity obj)
		{
			return InvokeTrigger(_update, obj);
		}

		/// <summary>
		///     Called when [delete].
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     True if the Obj is handeld
		/// </returns>
		public bool OnDelete(TEntity obj)
		{
			return InvokeTrigger(_delete, obj);
		}

		/// <summary>
		///     Occurs when [insert].
		/// </summary>
		private event EventHandler<IInsteadtOfActionToken<TEntity>> _insert;

		/// <summary>
		///     Occurs when [update].
		/// </summary>
		private event EventHandler<IInsteadtOfActionToken<TEntity>> _update;

		/// <summary>
		///     Occurs when [delete].
		/// </summary>
		private event EventHandler<IInsteadtOfActionToken<TEntity>> _delete;

		private bool InvokeTrigger(EventHandler<IInsteadtOfActionToken<TEntity>> trigger, TEntity obj)
		{
			if (_tabel == null)
			{
				return false;
			}

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

		/// <summary>
		///     Returns an Empty trigger collection
		/// </summary>
		/// <returns></returns>
		public static TriggerInsteadtOfCollection<TEntity> Empty()
		{
			return new TriggerInsteadtOfCollection<TEntity>();
		}
	}
}