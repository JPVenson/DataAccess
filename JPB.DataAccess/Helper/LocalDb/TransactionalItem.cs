using JPB.DataAccess.DbCollection;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal class TransactionalItem
	{
		internal object Item { get; set; }
		internal CollectionStates State { get; set; }

		internal TransactionalItem(object item, CollectionStates state)
		{
			Item = item;
			State = state;
		}
	}
}