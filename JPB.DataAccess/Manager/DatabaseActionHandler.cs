using JPB.DataAccess.DbEventArgs;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A database operation has to be done
	/// </summary>
	public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);
}