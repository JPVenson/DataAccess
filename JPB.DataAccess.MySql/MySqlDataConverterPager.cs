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

namespace JPB.DataAccess.MySql
{
    public class MySqlDataConverterPager<T, TE> : MySqlDataPager<T>, IWrapperDataPager<T,TE>
    {
	    /// <summary>
	    /// </summary>
	    public MySqlDataConverterPager()
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