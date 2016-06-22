/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Contacts.Pager
{
	public interface IDataPagerInfo
	{
		/// <summary>
		///     Id of Current page beween 1 and MaxPage
		/// </summary>
		long CurrentPage { get; set; }

		/// <summary>
		///     The last possible Page
		/// </summary>
		long MaxPage { get; }

		/// <summary>
		///     Items to load on one page
		/// </summary>
		int PageSize { get; set; }
	}

	/// <summary>
	///     Base pager
	/// </summary>
	public interface IDataPager : IDisposable, IDataPagerInfo
	{
		/// <summary>
		///     The most simple Select that produces result data. An aditional QueryCommand will wrap to enable Pageing, this so be aware
		///     of it
		/// </summary>
		IDbCommand BaseQuery { get; set; }

		/// <summary>
		///     Not Implimented
		/// </summary>
		bool Cache { get; set; }

		/// <summary>
		///     Should raise Events
		/// </summary>
		bool RaiseEvents { get; set; }

		/// <summary>
		///     Commands that are sequencely attached to the main pager command
		/// </summary>
		List<IDbCommand> AppendedComands { get; set; }

		/// <summary>
		///     Additional Commands to append to the base load command
		/// </summary>
		IEnumerable CurrentPageItems { get; }

		/// <summary>
		/// </summary>
		Action<Action> SyncHelper { get; set; }

		/// <summary>
		///     Raised if new Page is loading
		/// </summary>
		event Action NewPageLoading;

		/// <summary>
		///     Raised if new Page is loaded
		/// </summary>
		event Action NewPageLoaded;

		/// <summary>
		///     Loads the PageSize into CurrentPageItems
		/// </summary>
		void LoadPage(DbAccessLayer dbAccess);
	}

	/// <summary>
	///     Generic Data pager
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDataPager<T> : IDataPager
	{
		/// <summary>
		///     Typed list of all Elements
		/// </summary>
		new ICollection<T> CurrentPageItems { get; }


	}
}