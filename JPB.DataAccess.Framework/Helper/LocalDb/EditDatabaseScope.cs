#region

using JPB.DataAccess.Framework.Helper.LocalDb.Scopes;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb
{
	/// <summary>
	/// </summary>
	/// <seealso cref="DatabaseScope" />
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