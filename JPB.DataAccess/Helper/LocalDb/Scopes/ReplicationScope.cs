#region

using System;
using System.Transactions;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Scopes
{
	/// <summary>
	///     Defines a scope where a Replication can be done. This will disable all Trigger and Constraints and will reinvoke
	///     them when the scope is closed
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class ReplicationScope : IDisposable
	{
		[ThreadStatic] private static ReplicationScope _current;

		/// <summary>
		///     Initializes a new instance of the <see cref="ReplicationScope" /> class.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///     Nested Identity Scopes are not supported
		///     or
		///     Has to be executed inside a valid TransactionScope
		/// </exception>
		public ReplicationScope()
		{
			if (Current != null)
			{
				throw new InvalidOperationException("Nested Identity Scopes are not supported");
			}
			if (Transaction.Current == null)
			{
				throw new InvalidOperationException("Has to be executed inside a valid TransactionScope");
			}

			_current = this;

			foreach (var localDbReposetoryBase in LocalDbManager.Scope.Database)
			{
				localDbReposetoryBase.Value.IsMigrating = true;
			}
		}

		/// <summary>
		///     Gets the current Scope.
		/// </summary>
		/// <value>
		///     The current.
		/// </value>
		public static ReplicationScope Current
		{
			get { return _current; }
		}

		/// <summary>
		///     Submits all pending changes
		/// </summary>
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