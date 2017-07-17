namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IIdentifyerElementQuery : IQueryElement
	{
		/// <summary>
		///     Gets the current identifier.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		string CurrentIdentifier { get; }
	}
}