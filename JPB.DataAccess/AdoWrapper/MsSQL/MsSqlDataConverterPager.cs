using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper.MsSql
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
			base.NewPageLoaded += OnNewPageLoaded;
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

		public Func<T, TE> Converter { get; set; }

		public new ICollection<TE> CurrentPageItems { get; protected set; }

		private void OnNewPageLoaded()
		{
			CurrentPageItems.Clear();

			foreach (T currentPageItem in base.CurrentPageItems)
			{
				CurrentPageItems.Add(Converter(currentPageItem));
			}
		}
	}
}