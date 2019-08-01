using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Framework.Contacts.Pager;

namespace JPB.DataAccess.SqLite.NetStandard
{
	/// <summary>
	///     Converts all items from T to TE
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TE"></typeparam>
	public class SqLiteDataConverterPager<T, TE> :
		SqLiteDataPager<T>,
		IWrapperDataPager<T, TE>
	{
		/// <summary>
		/// </summary>
		public SqLiteDataConverterPager()
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

		public Func<T, TE> Converter { get; set; }

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