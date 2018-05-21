#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Transactions;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///		Contains all Properties to control the ThreadBased connection and Transaction behavior
	/// </summary>
	public class ThreadConnection
	{
		/// <summary>
		///		Ctor
		/// </summary>
		public ThreadConnection()
		{
			Transactions = new Queue<KeyValuePair<Transaction, IDbTransaction>>();
			LockRoot = new object();
		}

		internal Queue<KeyValuePair<Transaction, IDbTransaction>> Transactions { get; private set; }

		/// <summary>
		/// <see cref="IConnectionController.InstanceCounter"/>
		/// </summary>
		public long InstanceCounter { get; internal set; }
		/// <summary>
		/// <see cref="IConnectionController.LockRoot"/>
		/// </summary>
		public object LockRoot { get; private set; }

		/// <summary>
		/// <see cref="IConnectionController.Connection"/>
		/// </summary>
		public IDbConnection Connection { get; internal set; }
	}

	/// <summary>
	///     Controlls all Database Operations even for nested DataAccessLayers
	/// </summary>
	public class ThreadConnectionController : IConnectionController
	{
		private static readonly AsyncLocal<ThreadConnection> _threadConnectionInfo 
				= new AsyncLocal<ThreadConnection>();

		/// <summary>
		///		Contains infos about the Current ThreadConnection Infos
		/// </summary>
		public static ThreadConnection ThreadConnectionInfo
		{
			get { return _threadConnectionInfo.Value; }
			private set { _threadConnectionInfo.Value = value; }
		}

		/// <summary>
		///     ctor
		/// </summary>
		public ThreadConnectionController()
		{
			ThreadConnectionInfo = ThreadConnectionInfo ?? new ThreadConnection();
		}

		///// <summary>
		/////     Sets the Flag for using the <code>Transaction.Current</code> field.
		/////     If its once activated it cannot be deactivated as it affects all threads.
		/////     If its Activated you cannot use the <see cref="InstanceConnectionController" />
		/////     anymore for Nested Transaction.
		/////     as its not compatable with the <code>Transaction.Current</code>.
		/////     Thread Static
		///// </summary>
		//public static bool UseTransactionCurrent
		//{
		//	get { return ThreadConnection.UseTransactionCurrent; }
		//}

		/// <inheritdoc />
		public IDbTransaction Transaction
		{
			get { return ThreadConnectionInfo.Transactions.Count > 0 ? ThreadConnectionInfo.Transactions.Peek().Value : null; }
			set
			{
				if (value == null && ThreadConnectionInfo.Transactions.Count > 0)
				{
					ThreadConnectionInfo.Transactions.Dequeue();
					//if (UseTransactionCurrent)
					//{
					//	System.Transactions.Transaction.Current = ThreadConnectionInfo.Transactions.Count > 0 ? ThreadConnectionInfo.Transactions.Peek().Key : null;
					//}
				}
				else if (value != null)
				{
					//if (UseTransactionCurrent)
					//{
					//	System.Transactions.Transaction.Current = new CommittableTransaction(new TransactionOptions
					//	{
					//			Timeout = TimeSpan.MaxValue
					//	});
					//}

					ThreadConnectionInfo.Transactions.Enqueue(
					new KeyValuePair<Transaction, IDbTransaction>(System.Transactions.Transaction.Current, value));
				}
			}
		}

		/// <inheritdoc />
		public IDbConnection Connection
		{
			get { return ThreadConnectionInfo.Connection; }
			set { ThreadConnectionInfo.Connection = value; }
		}

		/// <inheritdoc />
		public long InstanceCounter
		{
			get { return ThreadConnectionInfo.InstanceCounter; }
			set { ThreadConnectionInfo.InstanceCounter = value; }
		}

		/// <inheritdoc />
		public object LockRoot
		{
			get { return ThreadConnectionInfo.LockRoot; }
		}

		/// <inheritdoc />
		public IConnectionController Clone()
		{
			return new ThreadConnectionController();
		}

		///// <summary>
		/////     Sets the UseTransactionCurrent to true
		///// </summary>
		//public static void UseTransactionClass()
		//{
		//	ThreadConnection.UseTransactionCurrent = true;
		//}
		private void ReleaseUnmanagedResources()
		{
			// TODO release unmanaged resources here
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			ReleaseUnmanagedResources();
			if (disposing)
			{
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc />
		~ThreadConnectionController()
		{
			Dispose(false);
		}
	}
}