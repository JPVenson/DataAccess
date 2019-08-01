namespace JPB.DataAccess.Framework.Query
{
	/// <summary>
	///     Enum for specifying the way in enumeration that is used by enumerating a IQueryContainer
	/// </summary>
	public enum EnumerationMode
	{
		/// <summary>
		///     The enumerator will convert the result that is obtained from the database immediately into a POCO
		/// </summary>
		FullOnLoad,

		/// <summary>
		///     Will bypass the current Complete loading logic and forces the DbAccessLayer to use a
		///     Lazy loading
		/// </summary>
		OnCall
	}
}