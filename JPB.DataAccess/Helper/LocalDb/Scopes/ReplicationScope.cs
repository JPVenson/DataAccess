using System;
using System.Transactions;

namespace JPB.DataAccess.Helper.LocalDb.Scopes
{
	public class ReplicationScope : IDisposable
	{
		public ReplicationScope()
		{
			if (Current != null)
				throw new InvalidOperationException("Nested Identity Scopes are not supported");
			if (Transaction.Current == null)
				throw new InvalidOperationException("Has to be executed inside a valid TransactionScope");

			_current = this;

			foreach (var localDbReposetoryBase in LocalDbManager.Scope.Database)
			{
				localDbReposetoryBase.Value.IsMigrating = true;
			}
		}

		[ThreadStatic]
		private static ReplicationScope _current;

		public static ReplicationScope Current
		{
			get { return _current; }
		}

		public void Dispose()
		{
			foreach (var localDbReposetoryBase in LocalDbManager.Scope.Database)
			{
				localDbReposetoryBase.Value.IsMigrating = false;
			}
			_current = null;
		}
	}
}