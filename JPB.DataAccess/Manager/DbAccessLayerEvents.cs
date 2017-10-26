#region

using System;
using System.Data;
using JPB.DataAccess.DbEventArgs;

#endregion

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A database operation has to be done
	/// </summary>
	public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);

	/// <summary>
	///     A database operation is done with an exception
	/// </summary>
	public delegate void DatabaseExceptionActionHandler(object sender, DatabaseActionEvent e, Exception ex);

	partial class DbAccessLayer
	{
		/// <summary>
		///     Should raise Instance bound Events
		/// </summary>
		public bool RaiseEvents { get; set; }

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

		internal void InvokeAsync(DatabaseActionHandler handler, object sender, IDbCommand query)
		{
			if (!RaiseEvents)
			{
				return;
			}

			if (handler != null)
			{
				var eventListeners = handler.GetInvocationList();
				foreach (var t in eventListeners)
				{
					var methodToInvoke = (DatabaseActionHandler) t;
					methodToInvoke.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebugger(Database)), ar => { }, null);
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

		internal void RaiseFailedQuery(object sender, IDbCommand query, Exception ex)
		{
			if (!RaiseEvents)
			{
				return;
			}

			var handler = OnFailedQuery;
			if (handler != null)
			{
				var eventListeners = handler.GetInvocationList();
				foreach (var t in eventListeners)
				{
					var methodToInvoke = (DatabaseExceptionActionHandler)t;
					methodToInvoke.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebugger(Database)), ex, ar => { }, null);
				}
			}
		}
	}
}