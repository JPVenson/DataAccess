#region

using JPB.DataAccess.Helper.LocalDb.Scopes;

#endregion

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Scopes.DatabaseScope" />
	public class EditDatabaseScope : DatabaseScope
	{
		internal EditDatabaseScope(LocalDbManager dbManager)
			: base(dbManager)
		{
			Drop = new DropCommand(this);
			Scope = dbManager;
		}

		internal LocalDbManager Scope { get; set; }

		/// <summary>
		///     Gets the drop command scope.
		/// </summary>
		/// <value>
		///     The drop.
		/// </value>
		public DropCommand Drop { get; private set; }
	}
}