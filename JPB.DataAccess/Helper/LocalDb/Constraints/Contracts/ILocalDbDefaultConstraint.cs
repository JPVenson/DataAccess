using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Contracts
{
	public interface ILocalDbDefaultConstraint : ILocalDbConstraint
	{
		void DefaultValue();
	}
}