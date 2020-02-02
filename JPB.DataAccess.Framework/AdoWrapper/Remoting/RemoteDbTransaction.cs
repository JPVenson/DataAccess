using System.Data;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Defines the wrapper for an external managed Transaction
	/// </summary>
	public class RemoteDbTransaction : IDbTransaction
	{
		/// <summary>
		///		The Associated Strategy
		/// </summary>
		public RemotingStrategy Strategy { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="isolationLevel"></param>
		/// <param name="strategy"></param>
		public RemoteDbTransaction(RemotingDbConnection connection, IsolationLevel isolationLevel, RemotingStrategy strategy)
		{
			Connection = connection;
			IsolationLevel = isolationLevel;
			Strategy = strategy;
			Status = TransactionStatus.InDoubt;
			Strategy.Events.OnTransactionCreated(this);
		}
		
		public enum TransactionStatus
		{
			Active,
			Committed,
			Aborted,
			InDoubt,
		}

		/// <summary>
		///		Gets in what status this transaction is currently in
		/// </summary>
		public TransactionStatus Status { get; private set; }

		/// <inheritdoc />
		public void Dispose()
		{
			if (Status != TransactionStatus.Committed && Status != TransactionStatus.Aborted)
			{
				Rollback();
			}
		}
		
		/// <inheritdoc />
		public void Commit()
		{
			if (Status == TransactionStatus.InDoubt)
			{
				Strategy.Events.OnTransactionCommit(this);
				Status = TransactionStatus.Committed;
			}
		}
		
		/// <inheritdoc />
		public void Rollback()
		{
			if (Status == TransactionStatus.InDoubt)
			{
				Strategy.Events.OnTransactionRollback(this);
				Status = TransactionStatus.Aborted;
			}
		}
		
		/// <inheritdoc />
		public IDbConnection Connection { get; }
		/// <inheritdoc />
		public IsolationLevel IsolationLevel { get; }
	}
}