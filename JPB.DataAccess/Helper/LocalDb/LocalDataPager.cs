using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// Provides LINQ paged access to an LocalDbReporsetory
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LocalDataPager<T> : IDataPager<T>
	{
		public LocalDataPager(LocalDbReposetory<T> localDbReposetory)
		{
			this.localDbReposetory = localDbReposetory;
			this.CurrentPage = 1;
			SyncHelper = (s) => s();
			CurrentPageItems = new ObservableCollection<T>();
		}

		private long _currentPage;
		public List<IDbCommand> AppendedComands { get; set; }

		public IDbCommand BaseQuery { get; set; }

		public bool Cache { get; set; }

		public long CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (value >= 1)
					_currentPage = value;
				else
				{
					throw new InvalidOperationException("The current page must be bigger or equals 1");
				}
			}
		}

		public ICollection<T> CurrentPageItems
		{
			get;
			private set;
		}

		public long MaxPage { get; private set; }

		public int PageSize { get; set; }

		public bool RaiseEvents { get; set; }

		public Action<Action> SyncHelper { get; set; }

		IEnumerable IDataPager.CurrentPageItems
		{
			get { return this.CurrentPageItems; }
		}

		public event Action NewPageLoaded;
		public event Action NewPageLoading;

		public void LoadPage(DbAccessLayer dbAccess)
		{
			SyncHelper(CurrentPageItems.Clear);
			MaxPage = localDbReposetory.Count / this.PageSize;
			if (RaiseEvents)
			{
				var handler = NewPageLoading;
				if (handler != null)
				{
					handler();
				}
			}

			var items = localDbReposetory.Skip((int)((this.CurrentPage - 1) * this.PageSize)).Take(this.PageSize).ToArray();

			foreach (var item in items)
			{
				SyncHelper(() =>
				{
					this.CurrentPageItems.Add(item);
				});
			}

			if (CurrentPage > MaxPage)
				CurrentPage = MaxPage;

			if (RaiseEvents)
			{
				var handler = NewPageLoaded;
				if (handler != null)
				{
					handler();
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		private LocalDbReposetory<T> localDbReposetory;


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