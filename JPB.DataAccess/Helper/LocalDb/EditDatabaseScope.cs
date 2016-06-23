using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Helper.LocalDb.Scopes;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class EditDatabaseScope : DatabaseScope
	{
		internal EditDatabaseScope(LocalDbManager dbManager) 
			: base(dbManager)
		{
			Drop = new DropCommand(this);
			Scope = dbManager;
		}

		internal LocalDbManager Scope { get; set; }

		public DropCommand Drop { get; private set; }
	}

	public class DropCommand
	{
		private readonly EditDatabaseScope _editDatabaseScope;

		public DropCommand(EditDatabaseScope editDatabaseScope)
		{
			_editDatabaseScope = editDatabaseScope;
		}

		public void Database(Type forType)
		{
			var firstOrDefault = _editDatabaseScope.Scope.Database.FirstOrDefault(s => s.Key == forType);
			_editDatabaseScope.Scope.Database.Remove(firstOrDefault.Key);
		}
	}
}
