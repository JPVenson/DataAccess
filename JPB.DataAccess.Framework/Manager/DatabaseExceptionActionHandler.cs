using System;
using JPB.DataAccess.Framework.DbEventArgs;

namespace JPB.DataAccess.Framework.Manager
{
	/// <summary>
	///     A database operation is done with an exception
	/// </summary>
	public delegate void DatabaseExceptionActionHandler(object sender, DatabaseActionEvent e, Exception ex);
}