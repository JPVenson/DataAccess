#region

using System;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///     Defines a new Transaction Scope where all changes to a local DB can be Rejected and Reverted
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class DefaultTransactionScope : IDisposable
	{
		private readonly IDatabase _db;

		/// <summary>
		///     Initializes a new instance of the <see cref="DefaultTransactionScope" /> class.
		/// </summary>
		/// <param name="db">The database.</param>
		public DefaultTransactionScope(IDatabase db)
		{
			_db = db;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_db.CloseConnection();
		}
	}
}