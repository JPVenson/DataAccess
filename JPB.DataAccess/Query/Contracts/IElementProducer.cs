namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IIdentifyerElementQuery" />
	public interface IElementProducer<out T> : IIdentifyerElementQuery
	{
	}
}