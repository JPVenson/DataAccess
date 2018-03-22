using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///
	/// </summary>
	public interface IStateQuery
	{
		/// <summary>
		///		The current Query state
		/// </summary>
		CondtionBuilderState State { get; }
	}
}