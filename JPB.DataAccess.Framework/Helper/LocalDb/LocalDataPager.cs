#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using JPB.DataAccess.Framework.Contacts.Pager;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.Query.Operators.Orders;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb
{
	/// <summary>
	///     Provides LINQ paged access to an LocalDbReporsetory
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="IDataPager{T}" />
	public class LocalDataPager<T> : IDataPager<T>
	{
		private int _currentPage;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDataPager{T}" /> class.
		/// </summary>
		/// <param name="localDbRepository">The local database repository.</param>
		public LocalDataPager(LocalDbRepository<T> localDbRepository)
		{
			_localDbRepository = localDbRepository;
			CurrentPage = 1;
			SyncHelper = s => s();
			CurrentPageItems = new ObservableCollection<T>();
		}

		/// <summary>
		///     Commands that are sequencely attached to the main pager command
		/// </summary>
		public List<IDbCommand> AppendedComands { get; set; }

		/// <summary>
		///     The most simple Select that produces result data. An aditional QueryCommand will wrap to enable Pageing, this so be
		///     aware
		///     of it
		/// </summary>
		public IDbCommand BaseQuery { get; set; }

		/// <summary>
		///     Not Implimented
		/// </summary>
		public bool Cache { get; set; }

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
		///     Typed list of all Elements
		/// </summary>
		public ICollection<T> CurrentPageItems { get; private set; }

		/// <inheritdoc />
		public OrderByColumn<T> CommandQuery { get; set; }

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
		///     Should raise Events
		/// </summary>
		public bool RaiseEvents { get; set; }

		/// <summary>
		/// </summary>
		public Action<Action> SyncHelper { get; set; }
		
		/// <summary>
		///     Raised if new Page is loaded
		/// </summary>
		public event Action NewPageLoaded;

		/// <summary>
		///     Raised if new Page is loading
		/// </summary>
		public event Action NewPageLoading;

		/// <summary>
		///     Loads the PageSize into CurrentPageItems
		/// </summary>
		/// <param name="dbAccess"></param>
		public void LoadPage(DbAccessLayer dbAccess)
		{
			SyncHelper(CurrentPageItems.Clear);
			MaxPage = (int) Math.Ceiling((decimal) _localDbRepository.Count / PageSize);
			if (RaiseEvents)
			{
				var handler = NewPageLoading;
				handler?.Invoke();
			}

			TotalItemCount = _localDbRepository.Count;

			var items = _localDbRepository.Skip((int) ((CurrentPage - 1) * PageSize)).Take(PageSize).ToArray();

			foreach (var item in items)
			{
				SyncHelper(() => { CurrentPageItems.Add(item); });
			}

			if (CurrentPage > MaxPage)
			{
				CurrentPage = MaxPage;
			}

			if (RaiseEvents)
			{
				var handler = NewPageLoaded;
				handler?.Invoke();
			}
		}

		#region IDisposable Support

		/// <summary>
		///     The disposed value
		/// </summary>
		private bool disposedValue; // To detect redundant calls

		/// <summary>
		///     The local database repository
		/// </summary>
		private readonly LocalDbRepository<T> _localDbRepository;


		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ILocalDataPager() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

		#endregion
	}
}