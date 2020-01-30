using System.Data;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public class RemoteDbTransaction : IDbTransaction
	{
		public RemotingStrategy Strategy { get; }

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

		public TransactionStatus Status { get; set; }

		public void Dispose()
		{
			if (Status != TransactionStatus.Committed && Status != TransactionStatus.Aborted)
			{
				Rollback();
			}
		}

		public void Commit()
		{
			if (Status == TransactionStatus.InDoubt)
			{
				Strategy.Events.OnTransactionCommit(this);
				Status = TransactionStatus.Committed;
			}
		}

		public void Rollback()
		{
			if (Status == TransactionStatus.InDoubt)
			{
				Strategy.Events.OnTransactionRollback(this);
				Status = TransactionStatus.Aborted;
			}
		}

		public IDbConnection Connection { get; }
		public IsolationLevel IsolationLevel { get; }
	}
}