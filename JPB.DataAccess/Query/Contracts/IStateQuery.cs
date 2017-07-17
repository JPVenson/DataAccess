using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///
	/// </summary>
	public interface IStateQuery
	{
		CondtionBuilderState State { get; }
	}
}