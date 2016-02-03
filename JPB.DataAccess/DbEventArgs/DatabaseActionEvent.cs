using System;
using JPB.DataAccess.DebuggerHelper;

namespace JPB.DataAccess.DbEventArgs
{
	/// <summary>
	/// Wraps an Event raised by the DbAccessLayer
	/// </summary>
	public class DatabaseActionEvent : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryDebugger"></param>
		public DatabaseActionEvent(QueryDebugger queryDebugger)
		{
			QueryDebugger = queryDebugger;
		}

		/// <summary>
		/// If enabled the QueryDebugger that contains the Query
		/// </summary>
		public QueryDebugger QueryDebugger { get; private set; }
	}
}