using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Contracts
{
	public interface ILocalDbUniqueConstraint : ILocalDbCheckConstraint
	{
		void Add(object item);
		void Delete(object item);
		void Update(object item);
	}
}