/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbEventArgs;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A database operation is done
	/// </summary>
	public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);

	/// <summary>
	///     A database operation is failed
	/// </summary>
	public delegate void DatabaseFailedActionHandler(object sender, Exception e);

	partial class DbAccessLayer
	{
		/// <summary>
		///     Should raise Instance bound Events
		/// </summary>
		public bool RaiseEvents { get; set; }

		/// <summary>
		///     Should raise events (Performace critical!)
		/// </summary>
		[Obsolete]
		public static bool RaiseStaticEvents { get; set; }

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
				return;

			if (handler != null)
			{
				var eventListeners = handler.GetInvocationList();
				foreach (Delegate t in eventListeners)
				{
					var methodToInvoke = (DatabaseActionHandler)t;
					methodToInvoke.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebugger(Database)), (ar => { }), null);
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
	}
}