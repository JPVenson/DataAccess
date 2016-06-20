using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	//public class InsteadtOfActionToken : EventArgs
	//{
	//	public InsteadtOfActionToken(object item, LocalDbReposetoryBase table)
	//	{
	//		Item = item;
	//		Table = table;
	//	}

	//	public virtual object Item { get; private set; }
	//	public LocalDbReposetoryBase Table { get; private set; }
	//}

	public class InsteadtOfActionToken<TEntity> : IInsteadtOfActionToken<TEntity>
	{
		public InsteadtOfActionToken(TEntity item, LocalDbReposetory<TEntity> table)
		{
			Item = item;
			Table = table;
		}

		public virtual TEntity Item { get; private set; }
		public LocalDbReposetory<TEntity> Table { get; private set; }
	}
}