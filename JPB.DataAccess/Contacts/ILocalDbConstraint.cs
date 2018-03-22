namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Defines a new Constraint that can be applyed to a Database
	/// </summary>
	public interface ILocalDbConstraint
	{
		/// <summary>
		///     The name of this Constraint
		/// </summary>
		string Name { get; }
	}
}