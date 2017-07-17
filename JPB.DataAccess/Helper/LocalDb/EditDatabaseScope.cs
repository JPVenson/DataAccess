#region

using System;
using System.Linq;
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

	/// <summary>
	/// </summary>
	public class DropCommand
	{
		private readonly EditDatabaseScope _editDatabaseScope;

		/// <summary>
		///     Initializes a new instance of the <see cref="DropCommand" /> class.
		/// </summary>
		/// <param name="editDatabaseScope">The edit database scope.</param>
		public DropCommand(EditDatabaseScope editDatabaseScope)
		{
			_editDatabaseScope = editDatabaseScope;
		}

		/// <summary>
		///     Databases the specified for type.
		/// </summary>
		/// <param name="forType">For type.</param>
		public void Database(Type forType)
		{
			var firstOrDefault = _editDatabaseScope.Scope.Database.FirstOrDefault(s => s.Key == forType);
			_editDatabaseScope.Scope.Database.Remove(firstOrDefault.Key);
		}
	}
}