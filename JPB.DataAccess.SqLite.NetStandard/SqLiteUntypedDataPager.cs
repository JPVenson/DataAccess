/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.Operators.Orders;

namespace JPB.DataAccess.SqLite
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SqLiteUntypedDataPager<T> : IDataPager<T>
	{
		private bool _cache;
		private int _currentPage;
		private Action<Action> _syncHelper;

		/// <summary>
		/// </summary>
		public SqLiteUntypedDataPager()
		{
			CurrentPage = 1;
			PageSize = 10;
			CurrentPageItems = new ObservableCollection<T>();

			SyncHelper = action => action();
		}

		public bool Cache
		{
			get { return _cache; }
			set
			{
				if (value)
				{
					throw new Exception("To be supported ... sory");
				}
				_cache = false;
			}
		}
		
		public bool RaiseEvents { get; set; }

		/// <summary>
		///     Raised if new Page is loading
		/// </summary>
		public event Action NewPageLoading;

		/// <summary>
		///     Raised if new page is Loaded
		/// </summary>
		public event Action NewPageLoaded;

		public int CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (value >= 1)
				{
					_currentPage = value;
				}
				else
				{
					throw new InvalidOperationException("The current page must be bigger or equals 1");
				}
			}
		}

		public int MaxPage { get; private set; }

		public int PageSize { get; set; }
		public long TotalItemCount { get; private set; }

		public ICollection<T> CurrentPageItems { get; protected set; }
		public OrderByColumn<T> CommandQuery { get; set; }
		public void LoadPage(DbAccessLayer dbAccess)
		{
			SyncHelper(CurrentPageItems.Clear);
			TotalItemCount = CommandQuery.CountInt().FirstOrDefault();
			MaxPage = (int)Math.Ceiling((decimal)TotalItemCount / PageSize);

			RaiseNewPageLoading();
			var elements = new SelectQuery<T>(dbAccess.Query()
					.WithCte(new ElementProducer<T>(CommandQuery
							.AsPagedQuery(CurrentPage, PageSize)),
						out var commandCte)
					.Select
					.Identifier<T>(commandCte))
				.ToArray();

			foreach (var item in elements)
			{
				var item1 = item;
				SyncHelper(() => CurrentPageItems.Add(item1));
			}

			RaiseNewPageLoaded();
		}

		/// <summary>
		/// </summary>
		public Action<Action> SyncHelper
		{
			get { return _syncHelper; }
			set
			{
				if (value != null)
				{
					_syncHelper = value;
				}
			}
		}

		/// <summary>
		/// </summary>
		protected virtual void RaiseNewPageLoaded()
		{
			if (!RaiseEvents)
			{
				return;
			}
			var handler = NewPageLoaded;
			if (handler != null)
			{
				handler();
			}
		}

		/// <summary>
		/// </summary>
		protected virtual void RaiseNewPageLoading()
		{
			if (!RaiseEvents)
			{
				return;
			}
			var handler = NewPageLoading;
			if (handler != null)
			{
				handler();
			}
		}

		public void Dispose()
		{
			CurrentPageItems.Clear();
		}
	}
}