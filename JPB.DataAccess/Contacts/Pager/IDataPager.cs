#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Contacts.Pager
{
	/// <summary>
	///     Base pager
	/// </summary>
	public interface IDataPager : IDisposable, IDataPagerInfo
	{
		/// <summary>
		///     Not Implimented
		/// </summary>
		bool Cache { get; set; }

		/// <summary>
		///     Should raise Events
		/// </summary>
		bool RaiseEvents { get; set; }
		
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
		ICollection<T> CurrentPageItems { get; }
		
		/// <summary>
		///     For Advanced querys including Order statements
		/// </summary>
		/// <value>
		///     The command query.
		/// </value>
		IElementProducer<T> CommandQuery { get; set; }

		/// <summary>
		///     Loads the PageSize into CurrentPageItems
		/// </summary>
		void LoadPage(DbAccessLayer dbAccess);
	}
}