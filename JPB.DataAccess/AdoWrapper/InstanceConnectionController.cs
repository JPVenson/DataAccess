using System.Collections.Generic;
using System.Data;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// Stores transactions for this IDatabase only
	/// </summary>
	public class InstanceConnectionController : IConnectionController
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public InstanceConnectionController()
		{
			LockRoot = new object();
			_transactions = new Queue<IDbTransaction>();
		}

		private readonly Queue<IDbTransaction> _transactions;

		/// <inheritdoc />
		public IDbTransaction Transaction
		{
			get { return _transactions.Count > 0 ? _transactions.Peek() : null; }
			set
			{
				if (value == null && _transactions.Count > 0)
				{
					_transactions.Dequeue();
				}
				else if (value != null)
				{
					_transactions.Enqueue(value);
				}
			}
		}

		/// <inheritdoc />
		public IDbConnection Connection { get; set; }

		/// <inheritdoc />
		public long InstanceCounter { get; set; }

		/// <inheritdoc />
		public object LockRoot { get; private set; }
	}
}