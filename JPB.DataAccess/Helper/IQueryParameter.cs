namespace JPB.DataAccess.Helper
{
	/// <summary>
	///     Wraps Paramters for Commands
	/// </summary>
	public interface IQueryParameter
	{
		string Name { get; set; }
		object Value { get; set; }
	}
}