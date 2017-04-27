namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Enum for specifying the way in enumeration that is used by enumerating a IQueryContainer
	/// </summary>
	public enum EnumerationMode
	{
		/// <summary>
		///     At the first call of GetEnumerator all items will be enumerated and stored
		///     Eager loading
		/// </summary>
		FullOnLoad,

		/// <summary>
		///     Will bypass the current Complete loading logic and forces the DbAccessLayer to use a
		///     Lazy loading
		/// </summary>
		OnCall
	}
}