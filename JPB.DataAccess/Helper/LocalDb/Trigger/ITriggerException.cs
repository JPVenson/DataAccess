namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	/// </summary>
	public interface ITriggerException
	{
		/// <summary>
		///     Gets the reason.
		/// </summary>
		/// <value>
		///     The reason or null.
		/// </value>
		string Reason { get; }
	}
}