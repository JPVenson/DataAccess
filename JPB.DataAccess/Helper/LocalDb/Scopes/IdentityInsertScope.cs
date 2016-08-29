using System;
using System.Transactions;

namespace JPB.DataAccess.Helper.LocalDb.Scopes
{
	/// <summary>
	/// Defines an Area that allows identity Inserts
	/// IDENTITY_INSERT on SQL
	/// </summary>
	public sealed class IdentityInsertScope : IDisposable
	{
		internal bool RewriteDefaultValues { get; private set; }

		[ThreadStatic]
		private static IdentityInsertScope _current;

		/// <summary>
		/// Creates a new Idenity Scope. Close it with Dispose
		/// Must be created inside an TransactionScope
		/// it is strongy recommanded to create this class inside an using construct!
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
				Current = new IdentityInsertScope(rewriteDefaultValues, true);
		}

		private IdentityInsertScope(bool rewriteDefaultValues = false, bool nested = false)
		{
			RewriteDefaultValues = rewriteDefaultValues;
		}

		/// <summary>
		/// The current Identity Scope
		/// </summary>
		public static IdentityInsertScope Current
		{
			get { return _current; }
			private set { _current = value; }
		}

		/// <summary>
		/// Occurs when [on identity insert completed].
		/// </summary>
		public event EventHandler OnIdentityInsertCompleted;

		internal void OnOnIdentityInsertCompleted()
		{
			var handler = OnIdentityInsertCompleted;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		///		Ends the Identity Insert and will trigger all indexes and ForgeinKey checks
		/// </summary>
		public void Dispose()
		{
			OnOnIdentityInsertCompleted();
			Current = null;
		}
	}
}