namespace JPB.DataAccess.DbCollection
{
	/// <summary>
	///     All states that an item inside an DbCollection can be
	/// </summary>
	public enum CollectionStates
	{
		/// <summary>
		///     Element request is not in store
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     Object was created from the Database and has not changed
		/// </summary>
		Unchanged,

		/// <summary>
		///     Object from UserCode
		/// </summary>
		Added,

		/// <summary>
		///     Object was created from the database and has changed since then
		/// </summary>
		Changed,

		/// <summary>
		///     Object was created from the database and should be created
		/// </summary>
		Removed
	}
}