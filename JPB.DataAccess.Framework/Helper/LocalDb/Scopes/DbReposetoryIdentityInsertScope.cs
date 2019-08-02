#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Scopes
{
	/// <summary>
	///     Base class for Identity Operations on ether a Database or a LocalDbRepository
	/// </summary>
	public abstract class IdentityInsertScope : IDisposable
	{
		/// <summary>
		///     Creates a new Idenity Scope. Close it with Dispose
		///     Must be created inside an Transaction or TransactionScope ( for LocalDbRepositorys )
		///     it is strongy recommanded to create this class inside an using construct!
		/// </summary>
		protected IdentityInsertScope()
		{
			Init();
		}

		private void Init()
		{
			if (GetStore() != null)
			{
				throw new InvalidOperationException("Nested Identity Scopes are not supported");
			}

			IsInValidTransaction = Transaction.Current != null;

			if (GetStore() == null)
			{
				SetStore(this);
			}

			SetOnTables = GetStore().SetOnTables ?? new HashSet<string>();
		}

		internal bool IsInValidTransaction { get; private set; }

		internal ICollection<string> SetOnTables { get; set; }

		/// <summary>
		/// </summary>
		internal abstract IdentityInsertScope GetStore();

		/// <summary>
		/// </summary>
		internal abstract void SetStore(IdentityInsertScope item);

		internal void EnsureTransaction()
		{
			if (!IsInValidTransaction)
			{
				throw new InvalidOperationException("Has to be executed inside a valid TransactionScope");
			}
		}

		/// <summary>
		///     Ends the Identity Insert and will trigger all indexes and ForgeinKey checks
		/// </summary>
		public void Dispose(bool isDisposing)
		{
			try
			{
				OnOnIdentityInsertCompleted();
			}
			finally
			{
				SetStore(null);
			}
		}

		/// <summary>
		///     Occurs when [on identity insert completed].
		/// </summary>
		public event EventHandler OnIdentityInsertCompleted;

		internal void OnOnIdentityInsertCompleted()
		{
			var handler = OnIdentityInsertCompleted;
			handler?.Invoke(this, EventArgs.Empty);
		}

		internal void EnableIdentityModfiy(string classInfoTableName, IDatabase db)
		{
			if (SetOnTables.Contains(classInfoTableName))
			{
				return;
			}

			EventHandler onOnIdentityInsertCompleted = null;
			onOnIdentityInsertCompleted = (sender, e) =>
			{
				OnIdentityInsertCompleted -= onOnIdentityInsertCompleted;
				var disableIdentityInsert = db.Strategy.DisableIdentityInsert(classInfoTableName, db.GetConnection());
				if (disableIdentityInsert != null)
				{
					db.ExecuteNonQuery(disableIdentityInsert);
				}
			};
			OnIdentityInsertCompleted += onOnIdentityInsertCompleted;
			var enableIdentityInsert = db.Strategy.EnableIdentityInsert(classInfoTableName, db.GetConnection());
			if (enableIdentityInsert != null)
			{
				db.ExecuteNonQuery(enableIdentityInsert);
			}
		}

		private void ReleaseUnmanagedResources()
		{
			// TODO release unmanaged resources here
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc />
		~IdentityInsertScope()
		{
			Dispose(false);
		}
	}

	/// <summary>
	///     Defines an Area that allows identity Inserts on a LocalDbRepository
	///     IDENTITY_INSERT on SQL
	///     NOT THREAD SAVE
	/// </summary>
	public sealed class DbReposetoryIdentityInsertScope : IdentityInsertScope
	{
		[ThreadStatic]
		internal static DbReposetoryIdentityInsertScope Current;

		/// <summary>
		///		Creates a New <code>DbReposetoryIdentityInsertScope</code> or obtains the current active Instance
		/// </summary>
		/// <returns></returns>
		public static DbReposetoryIdentityInsertScope CreateOrObtain(bool rewriteDefaultValues = false)
		{
			if (Current != null)
			{
				return Current;
			}

			var newScope = new DbReposetoryIdentityInsertScope(rewriteDefaultValues);
			try
			{
				newScope.EnsureTransaction();
				return newScope;
			}
			catch
			{
				newScope.Dispose();
				throw;
			}
		}

		/// <summary>
		///     Creates a new Idenity Scope. Close it with Dispose
		///     Must be created inside an Transaction or TransactionScope ( for LocalDbRepositorys )
		///     it is strongy recommanded to create this class inside an using construct!
		/// </summary>
		/// <param name="rewriteDefaultValues">Should every DefaultValue still be set to a valid Id</param>
		private DbReposetoryIdentityInsertScope(bool rewriteDefaultValues = false)
		{
			RewriteDefaultValues = rewriteDefaultValues;
		}

		internal bool RewriteDefaultValues { get; private set; }

		internal override IdentityInsertScope GetStore()
		{
			return Current;
		}

		internal override void SetStore(IdentityInsertScope item)
		{
			Current = (DbReposetoryIdentityInsertScope) item;
		}
	}

	/// <summary>
	///     Defines an Area that allows identity Inserts on a Database
	///     IDENTITY_INSERT on SQL
	///     NOT THREAD SAVE
	/// </summary>
	public sealed class DbIdentityInsertScope : IdentityInsertScope
	{
		[ThreadStatic]
		private static DbIdentityInsertScope _current;
		public static DbIdentityInsertScope Current
		{
			get { return _current; }
			internal set { _current = value; }
		}

		internal override IdentityInsertScope GetStore()
		{
			return Current;
		}

		internal override void SetStore(IdentityInsertScope item)
		{
			Current = (DbIdentityInsertScope) item;
		}
	}
}