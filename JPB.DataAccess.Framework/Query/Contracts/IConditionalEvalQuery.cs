namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="IQueryElement" />
	public interface IConditionalEvalQuery<out T> : IQueryElement, IStateQuery
	{
	}
}