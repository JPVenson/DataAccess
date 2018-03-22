#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.Contacts.Pager
{
	/// <summary>
	///     Base pager
	/// </summary>
	public interface IDataPager : IDisposable, IDataPagerInfo
	{
		/// <summary>
		///     The most simple Select that produces result data. An aditional QueryCommand will wrap to enable Pageing, this so be
		///     aware
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