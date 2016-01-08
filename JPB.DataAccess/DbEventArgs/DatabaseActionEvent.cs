using System;
using JPB.DataAccess.DebuggerHelper;

namespace JPB.DataAccess.DbEventArgs
{
	public class DatabaseActionEvent : EventArgs
	{
		public DatabaseActionEvent(QueryDebugger queryDebugger)
		{
			QueryDebugger = queryDebugger;
		}

		public QueryDebugger QueryDebugger { get; private set; }
	}
}