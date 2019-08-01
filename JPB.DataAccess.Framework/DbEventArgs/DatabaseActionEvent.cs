#region

using System;
using JPB.DataAccess.Framework.DebuggerHelper;

#endregion

namespace JPB.DataAccess.Framework.DbEventArgs
{
	/// <summary>
	///     Wraps an Event raised by the DbAccessLayer
	/// </summary>
	public class DatabaseActionEvent : EventArgs
	{
		/// <summary>
		/// </summary>
		/// <param name="queryDebugger"></param>
		public DatabaseActionEvent(QueryDebugger queryDebugger)
		{
			QueryDebugger = queryDebugger;
		}

		/// <summary>
		///     If enabled the QueryDebugger that contains the QueryCommand
		/// </summary>
		public QueryDebugger QueryDebugger { get; private set; }
	}
}