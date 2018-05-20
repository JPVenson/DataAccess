using System;
using System.Data;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// Defines how Transactions are handeld
	/// </summary>
	public interface IConnectionController : IDisposable
	{
		/// <summary>
		/// Returns a Transaction or null
		/// </summary>
		IDbTransaction Transaction { get; set; }

		/// <summary>
		/// Returns a Transaction or null
		/// </summary>
		IDbConnection Connection { get; set; }

		/// <summary>
		/// Keeps track over all Connection attempts made. When it reaches 0 the connection should be finnaly closed and all still open Transactions should be rolled back.
		/// </summary>
		long InstanceCounter { get; set; }

		/// <summary>
		/// The sync root for Parallel access
		/// </summary>
		object LockRoot { get; }
	}
}