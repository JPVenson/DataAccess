namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{T}" />
	public interface IConditionalQuery<out T> : IElementProducer<T>
	{
	}
}