namespace JPB.DataAccess.Framework.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="IIdentifyerElementQuery" />
	public interface IElementProducer<out T> : IIdentifyerElementQuery
	{
	}
}