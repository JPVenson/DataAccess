using System;
using System.Collections.Generic;
using System.Data;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	/// Controlls all Database Operations even for nested DataAccessLayers
	/// </summary>
	public class ThreadConnectionController : IConnectionController
	{
		[ThreadStatic]
		private static Queue<IDbTransaction> _transactions;

		[ThreadStatic]
		private static long _instanceCounter;

		[ThreadStatic]
		private static object _lockRoot;

		[ThreadStatic]
		private static IDbConnection _connection;

		/// <summary>
		/// ctor
		/// </summary>
		public ThreadConnectionController()
		{
			_transactions = _transactions ?? new Queue<IDbTransaction>();
			_lockRoot = _lockRoot ?? new object();
		}

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
		public IDbConnection Connection
		{
			get { return _connection; }
			set { _connection = value; }
		}

		/// <inheritdoc />
		public long InstanceCounter
		{
			get { return _instanceCounter; }
			set { _instanceCounter = value; }
		}

		/// <inheritdoc />
		public object LockRoot
		{
			get { return _lockRoot; }
		}
	}
}