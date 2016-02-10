using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// 
	/// </summary>
	public class DatabaseScope : IDisposable
	{
		public DatabaseScope()
		{
			if (LocalDbManager.Scope != null)
				throw new NotSupportedException("Nested DatabaseScopes are not allowed");

			LocalDbManager.Scope = new LocalDbManager();
		}

		public void Dispose()
		{
			LocalDbManager.Scope = null;
		}
	}
}