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
		public static event DatabaseActionHandler OnDelete;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Select Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public static event DatabaseActionHandler OnSelect;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Update Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public static event DatabaseActionHandler OnUpdate;

		/// <summary>
		///     Will be triggerd when any DbAccessLayer tries to handle a Insert Statement.
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public static event DatabaseActionHandler OnInsert;

		/// <summary>
		///     Will be triggerd when any exception is thrown !Related! to the XML Parsin process
		///     Will only be triggerd when setting RaiseStaticEvents to true
		/// </summary>
		public static event DatabaseFailedActionHandler OnException;

		protected internal static void RaiseDelete(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnDelete;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		protected internal static void RaiseException(object sender, Exception ex)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnException;
			if (handler != null)
				handler.BeginInvoke(sender, ex, s => { }, null);
		}

		protected internal static void RaiseSelect(IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;
			var handler = OnSelect;
			if (handler != null)
				handler.BeginInvoke(null, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		protected internal static void RaiseUpdate(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnUpdate;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}

		protected internal static void RaiseInsert(object sender, IDbCommand query, IDatabase source)
		{
			if (!RaiseStaticEvents)
				return;

			var handler = OnInsert;
			if (handler != null)
				handler.BeginInvoke(sender, new DatabaseActionEvent(query.CreateQueryDebuggerAuto(source)), s => { }, null);
		}
	}
}