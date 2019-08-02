#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Contacts.Pager;

#endregion

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	///     Converts all items from T to TE
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TE"></typeparam>
	public class MsSqlDataConverterPager<T, TE> :
		MsSqlDataPager<T>,
		IWrapperDataPager<T, TE>
	{
		/// <summary>
		/// </summary>
		public MsSqlDataConverterPager()
		{
			SyncHelper = action => action();
			NewPageLoaded += OnNewPageLoaded;
			base.RaiseEvents = true;
			CurrentPageItems = new ObservableCollection<TE>();
		}

		/// <summary>
		/// </summary>
		public new bool RaiseEvents
		{
			get { return true; }
			set { }
		}

		/// <summary>
		///     Function to convert all items from T to TE
		/// </summary>
		public Func<T, TE> Converter { get; set; }

		/// <summary>
		///     Gets or sets the current page items.
		/// </summary>
		/// <value>
		///     The current page items.
		/// </value>
		public new ICollection<TE> CurrentPageItems { get; protected set; }

		private void OnNewPageLoaded()
		{
			CurrentPageItems.Clear();

			foreach (var currentPageItem in base.CurrentPageItems)
			{
				CurrentPageItems.Add(Converter(currentPageItem));
			}
		}
	}
}