#region

using System.Collections.Generic;

#endregion

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="MsSqlUntypedDataPager{T}" />
	public class MsSqlDataPager<T> : MsSqlUntypedDataPager<T>
	{
		/// <summary>
		///     Gets the current page items.
		/// </summary>
		/// <value>
		///     The current page items.
		/// </value>
		public new ICollection<T> CurrentPageItems
		{
			get { return base.CurrentPageItems; }
		}
	}
}