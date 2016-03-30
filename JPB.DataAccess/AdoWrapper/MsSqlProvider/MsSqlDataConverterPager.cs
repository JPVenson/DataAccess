/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Contacts.Pager;

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
			base.NewPageLoaded += OnNewPageLoaded;
			base.RaiseEvents = true;
			CurrentPageItems = new ObservableCollection<TE>();
		}

		/// <summary>
		/// </summary>
		public bool RaiseEvents
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