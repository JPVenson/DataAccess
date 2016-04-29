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
	///     A database operation is done
	/// </summary>
	public delegate void DatabaseFailedActionHandler(object sender, Exception e);

	partial class DbAccessLayer
	{
		/// <summary>
		///     Should raise Instance bound Events
		/// </summary>
		[Obsolete]
		public bool RaiseEvents { get; set; }

		/// <summary>
		///     Should raise non Instance bound Events
		/// </summary>
		public static bool RaiseStaticEvents { get; set; }

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Delete Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public event DatabaseActionHandler OnDelete;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Select Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public event DatabaseActionHandler OnSelect;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Update Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public event DatabaseActionHandler OnUpdate;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Insert Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public event DatabaseActionHandler OnInsert;

		internal void RaiseDelete(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnDelete;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		internal void RaiseSelect(IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;
			var handler = OnSelect;
			if (handler != null)
				handler.BeginInvoke(null, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		internal void RaiseUpdate(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnUpdate;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		internal void RaiseInsert(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnInsert;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}
	}
}