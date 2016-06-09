using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// Provides a logical scope for defining databases. All DbCollections within this scope are logicly combined. 
	/// Use the Using keyword to ensure correctness
	/// </summary>
	public class DatabaseScope : IDisposable
	{
		/// <summary>
		/// Creates a new Database
		/// </summary>
		public DatabaseScope()
		{
			if (LocalDbManager.Scope != null)
				throw new NotSupportedException("Nested DatabaseScopes are not allowed");

			LocalDbManager.Scope = new LocalDbManager();
		}

		/// <summary>
		/// Will be invoked if dispose is called
		/// </summary>
		public event EventHandler SetupDone
		{
			add { LocalDbManager.Scope.SetupDone += value; }
			remove { LocalDbManager.Scope.SetupDone -= value; }
		}

		public void Dispose()
		{
			LocalDbManager.Scope.OnSetupDone();
			LocalDbManager.Scope = null;
		}
	}
}