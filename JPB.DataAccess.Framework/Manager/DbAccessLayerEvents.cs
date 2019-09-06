#region

using System;
using System.Data;
using JPB.DataAccess.DbEventArgs;

#endregion

namespace JPB.DataAccess.Manager
{
	partial class DbAccessLayer
	{
		/// <summary>
		///     Should raise Instance bound Events
		/// </summary>
		public bool RaiseEvents { get; set; }

		/// <summary>
		///		Can be used in conjunction with the <seealso cref="RaiseEvents"/> flag. If set to true events will be invoked async
		///		Default is True
		/// </summary> 
		public bool RaiseEventsAsync { get; set; } = true;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer detects an invalid Query that failed on the server
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseExceptionActionHandler OnFailedQuery;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Delete Statement.
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseActionHandler OnDelete;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Select Statement.
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseActionHandler OnSelect;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Update Statement.
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseActionHandler OnUpdate;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Insert Statement.
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseActionHandler OnInsert;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Statement that has no result.
		///     Will only be triggerd when setting RaiseEvents to true
		/// </summary>
		public event DatabaseActionHandler OnNonResultQuery;

		/// <summary>
		///		Will be triggered when any Event was raised
		/// </summary>
		public event OnDatabaseActionHandler HandlerRaised;

		internal void InvokeAsync(DatabaseActionHandler handler, object sender, IDbCommand query)
		{
			if (!RaiseEvents)
			{
				return;
			}

			if (handler != null)
			{
				if (Async && !ThreadSave && RaiseEventsAsync)
				{
					var eventListeners = handler.GetInvocationList();
					foreach (var t in eventListeners)
					{
						var methodToInvoke = (DatabaseActionHandler)t;
						var databaseActionEvent = new DatabaseActionEvent(query.CreateQueryDebugger(Database));
						methodToInvoke.BeginInvoke(sender, databaseActionEvent, ar =>
						{
							OnHandlerRaised(databaseActionEvent, handler);
						}, null);
					}
				}
				else
				{
					var eventArgs = new DatabaseActionEvent(query.CreateQueryDebugger(Database));
					handler(sender, eventArgs);
					OnHandlerRaised(eventArgs, handler);
				}
			}
		}

		internal void RaiseDelete(object sender, IDbCommand query)
		{
			InvokeAsync(OnDelete, sender, query);
		}

		internal void RaiseSelect(IDbCommand query)
		{
			InvokeAsync(OnSelect, this, query);
		}

		internal void RaiseUpdate(object sender, IDbCommand query)
		{
			InvokeAsync(OnUpdate, sender, query);
		}

		internal void RaiseInsert(object sender, IDbCommand query)
		{
			InvokeAsync(OnInsert, sender, query);
		}

		internal void RaiseNoResult(object sender, IDbCommand query)
		{
			InvokeAsync(OnNonResultQuery, sender, query);
		}

		internal void RaiseFailedQuery(object sender, IDbCommand query, Exception ex)
		{
			if (!RaiseEvents)
			{
				return;
			}

			var handler = OnFailedQuery;
			if (handler != null)
			{
				if (Async && !ThreadSave)
				{
					var eventListeners = handler.GetInvocationList();
					foreach (var t in eventListeners)
					{
						var methodToInvoke = (DatabaseExceptionActionHandler)t;
						methodToInvoke.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebugger(Database)), ex, ar => { }, null);
					}
				}
				else
				{
					handler(sender, new DatabaseActionEvent(query.CreateQueryDebugger(Database)), ex);
				}
			}
		}

		internal void OnHandlerRaised(DatabaseActionEvent e, DatabaseActionHandler eventhandler)
		{
			var handler = HandlerRaised;
			handler?.Invoke(this, e, eventhandler);
		}
	}
}