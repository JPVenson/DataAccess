namespace JPB.DataAccess.Contacts.Pager
{
	/// <summary>
	///     Defines the Output of an Pager
	/// </summary>
	public interface IDataPagerInfo
	{
		/// <summary>
		///     Id of Current page beween 1 and MaxPage
		/// </summary>
		int CurrentPage { get; set; }

		/// <summary>
		///     The last possible Page
		/// </summary>
		int MaxPage { get; }

		/// <summary>
		///     Items to load on one page
		/// </summary>
		int PageSize { get; set; }

		/// <summary>
		///     Get the complete ammount of all items listend
		/// </summary>
		long TotalItemCount { get; }
	}
}