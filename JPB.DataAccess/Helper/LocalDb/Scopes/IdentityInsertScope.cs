#region

using System;
using System.Transactions;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Scopes
{
	/// <summary>
	///     Defines an Area that allows identity Inserts
	///     IDENTITY_INSERT on SQL
	///     NOT THREAD SAVE
	/// </summary>
	public sealed class IdentityInsertScope : IDisposable
	{
		[ThreadStatic] private static IdentityInsertScope _current;

		/// <summary>
		///     Creates a new Idenity Scope. Close it with Dispose
		///     Must be created inside an TransactionScope
		///     it is strongy recommanded to create this class inside an using construct!
		/// </summary>
		/// <param name="rewriteDefaultValues">Should every DefaultValue still be set to a valid Id</param>
		public IdentityInsertScope(bool rewriteDefaultValues = false)
		{
			RewriteDefaultValues = rewriteDefaultValues;
			if (Current != null)
				throw new InvalidOperationException("Nested Identity Scopes are not supported");
			if (Transaction.Current == null)
				throw new InvalidOperationException("Has to be executed inside a valid TransactionScope");

			if (Current == null)
				Current = this;
		}

		internal bool RewriteDefaultValues { get; private set; }

		/// <summary>
		///     The current Identity Scope
		/// </summary>
		public static IdentityInsertScope Current
		{
			get { return _current; }
			private set { _current = value; }
		}

		/// <summary>
		///     Ends the Identity Insert and will trigger all indexes and ForgeinKey checks
		/// </summary>
		public void Dispose()
		{
			OnOnIdentityInsertCompleted();
			Current = null;
		}

		/// <summary>
		///     Occurs when [on identity insert completed].
		/// </summary>
		public event EventHandler OnIdentityInsertCompleted;

		internal void OnOnIdentityInsertCompleted()
		{
			var handler = OnIdentityInsertCompleted;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}
	}
}