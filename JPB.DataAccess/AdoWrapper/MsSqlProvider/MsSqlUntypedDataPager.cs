#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.Operators.Orders;

#endregion

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.Pager.IDataPager{T}" />
	public class MsSqlUntypedDataPager<T> : IDataPager<T>
	{
		/// <summary>
		///     The cache
		/// </summary>
		private bool _cache;

		/// <summary>
		///     The check run
		/// </summary>
		private bool? _checkRun;

		/// <summary>
		///     The current page
		/// </summary>
		private int _currentPage;

		/// <summary>
		///     The synchronize helper
		/// </summary>
		private Action<Action> _syncHelper;
		
		/// <summary>
		///     The SQL version
		/// </summary>
		protected string SqlVersion;

		/// <summary>
		///     Initializes a new instance of the <see cref="MsSqlUntypedDataPager{T}" /> class.
		/// </summary>
		public MsSqlUntypedDataPager()
		{
			CurrentPage = 1;
			PageSize = 10;
			CurrentPageItems = new ObservableCollection<T>();
			SyncHelper = action => action();
		}

		/// <summary>
		///     For Advanced querys including Order statements
		/// </summary>
		/// <value>
		///     The command query.
		/// </value>
		public OrderByColumn<T> CommandQuery { get; set; }

		/// <summary>
		///     Not Implimented
		/// </summary>
		/// <exception cref="Exception">To be supported ... sory</exception>
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

		/// <summary>
		///     Should raise Events
		/// </summary>
		public bool RaiseEvents { get; set; }

		/// <summary>
		///     Raised if new Page is loading
		/// </summary>
		public event Action NewPageLoading;

		/// <summary>
		///     Raised if new page is Loaded
		/// </summary>
		public event Action NewPageLoaded;

		/// <summary>
		///     Id of Current page beween 1 and MaxPage
		/// </summary>
		/// <exception cref="InvalidOperationException">The current page must be bigger or equals 1</exception>
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

		/// <summary>
		///     The last possible Page
		/// </summary>
		public int MaxPage { get; private set; }

		/// <summary>
		///     Items to load on one page
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		///     Get the complete ammount of all items listend
		/// </summary>
		public long TotalItemCount { get; private set; }

		/// <summary>
		///     Typed list of all Elements
		/// </summary>
		public ICollection<T> CurrentPageItems { get; protected set; }

		/// <summary>
		///     Loads the PageSize into CurrentPageItems
		/// </summary>
		/// <param name="dbAccess"></param>
		void IDataPager<T>.LoadPage(DbAccessLayer dbAccess)
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
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			CurrentPageItems.Clear();
		}

		/// <summary>
		///     Raises the new page loaded.
		/// </summary>
		protected virtual void RaiseNewPageLoaded()
		{
			if (!RaiseEvents)
			{
				return;
			}

			var handler = NewPageLoaded;
			handler?.Invoke();
		}

		/// <summary>
		///     Raises the new page loading.
		/// </summary>
		protected virtual void RaiseNewPageLoading()
		{
			if (!RaiseEvents)
			{
				return;
			}

			var handler = NewPageLoading;
			handler?.Invoke();
		}

		/// <summary>
		///     Checks the version for fetch.
		/// </summary>
		/// <returns></returns>
		private bool CheckVersionForFetch()
		{
			if (_checkRun != null)
			{
				return _checkRun.Value;
			}

			var versionParts = SqlVersion.Split('.');

			//Target 11.0.2100.60 or higher
			//      Major Version
			//      Minor Version
			//      Build Number
			//      Revision

			var major = int.Parse(versionParts[0]);
			var minor = int.Parse(versionParts[1]);
			var build = int.Parse(versionParts[2]);
			var revision = int.Parse(versionParts[3]);

			if (major > 11)
			{
				_checkRun = true;
			}
			else if (major == 11)
			{
				if (minor > 0)
				{
					_checkRun = true;
				}
				else if (minor == 0)
				{
					if (build > 2100)
					{
						_checkRun = true;
					}
					else if (build == 2100)
					{
						if (revision >= 60)
						{
							_checkRun = true;
						}
					}
				}
			}
			else
			{
				_checkRun = false;
			}

			return _checkRun != null && _checkRun.Value;
		}
	}
}