namespace JPB.DataAccess.Contacts
{
	/// <summary>
	/// </summary>
	public enum LoadStrategy
	{
		/// <summary>
		///     Tells the API to include the field name into a Requested Select
		/// </summary>
		IncludeInSelect,

		/// <summary>
		///     Tells the API that the field should be loaded Implizit
		///     If you do select the field with your own statement the xml will not be parsed
		/// </summary>
		NotIncludeInSelect
	}
}