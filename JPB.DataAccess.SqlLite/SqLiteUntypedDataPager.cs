/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;

namespace JPB.DataAccess.SqLite
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SqLiteUntypedDataPager<T> : IDataPager<T>
	{
		private bool _cache;
		private long _currentPage;
		private Action<Action> _syncHelper;

		/// <summary>
		/// </summary>
		public SqLiteUntypedDataPager()
		{
			CurrentPage = 1;
			PageSize = 10;
			AppendedComands = new List<IDbCommand>();
			CurrentPageItems = new ObservableCollection<T>();

			SyncHelper = action => action();
		}

		/// <summary>
		///     Internal Use
		/// </summary>
		public Type TargetType { get; set; }

		public bool Cache
		{
			get { return _cache; }
			set
			{
				if (value)
					throw new Exception("To be supported ... sory");
				_cache = false;
			}
		}

		public IDbCommand BaseQuery { get; set; }

		public bool RaiseEvents { get; set; }

		/// <summary>
		///     Raised if new Page is loading
		/// </summary>
		public event Action NewPageLoading;

		/// <summary>
		///     Raised if new page is Loaded
		/// </summary>
		public event Action NewPageLoaded;

		public List<IDbCommand> AppendedComands { get; set; }

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

		public long MaxPage { get; private set; }

		public int PageSize { get; set; }

		public ICollection<T> CurrentPageItems { get; protected set; }

		void IDataPager.LoadPage(DbAccessLayer dbAccess)
		{
			T[] selectWhere = null;
			IDbCommand finalAppendCommand;
			if (AppendedComands.Any())
			{
				if (BaseQuery == null)
				{
					BaseQuery = dbAccess.CreateSelect<T>();
				}

				finalAppendCommand = AppendedComands.Aggregate(BaseQuery,
					(current, comand) => dbAccess.Database.MergeTextToParameters(current, comand, false, 1, true, false));
			}
			else
			{
				if (BaseQuery == null)
				{
					BaseQuery = dbAccess.CreateSelect<T>();
				}

				finalAppendCommand = BaseQuery;
			}

			SyncHelper(CurrentPageItems.Clear);

			var pk = TargetType.GetPK(dbAccess.Config);

			var selectMaxCommand = dbAccess
				.Query()
				.WithCte("CTE", f =>
				{
					f.QueryCommand(finalAppendCommand);
				})
				.QueryText("SELECT COUNT(*) FROM CTE")
				.ContainerObject
				.Compile();

			var maxItems = dbAccess.RunPrimetivSelect(typeof(long), selectMaxCommand).FirstOrDefault();
			if (maxItems != null)
			{
				long parsedCount;
				long.TryParse(maxItems.ToString(), out parsedCount);
				MaxPage = (long)Math.Ceiling((decimal)parsedCount / PageSize);
			}

			RaiseNewPageLoading();
			IDbCommand command;

			command = dbAccess.Query()
					.WithCte("CTE", cte => cte.QueryCommand(finalAppendCommand))
					.QueryText("SELECT * FROM CTE")
					.QueryText("ORDER BY " + pk)
					.QueryD("ASC LIMIT @PageSize OFFSET @PagedRows", new
					{
						PagedRows = CurrentPage * PageSize,
						PageSize
					})
					.ContainerObject
					.Compile();

			selectWhere = dbAccess.SelectNative(TargetType, command, true).Cast<T>().ToArray();

			foreach (T item in selectWhere)
			{
				var item1 = item;
				SyncHelper(() => CurrentPageItems.Add(item1));
			}

			if (CurrentPage > MaxPage)
				CurrentPage = MaxPage;

			RaiseNewPageLoaded();
		}

		IEnumerable IDataPager.CurrentPageItems
		{
			get { return CurrentPageItems; }
		}

		/// <summary>
		/// </summary>
		public Action<Action> SyncHelper
		{
			get { return _syncHelper; }
			set
			{
				if (value != null)
					_syncHelper = value;
			}
		}

		/// <summary>
		/// </summary>
		protected virtual void RaiseNewPageLoaded()
		{
			if (!RaiseEvents)
				return;
			var handler = NewPageLoaded;
			if (handler != null) handler();
		}

		/// <summary>
		/// </summary>
		protected virtual void RaiseNewPageLoading()
		{
			if (!RaiseEvents)
				return;
			var handler = NewPageLoading;
			if (handler != null) handler();
		}
		
		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}