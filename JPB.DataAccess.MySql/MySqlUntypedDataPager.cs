#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;

#endregion

namespace JPB.DataAccess.MySql
{
	public class MySqlUntypedDataPager<T> : IDataPager<T>
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
		private long _currentPage;

		/// <summary>
		///     The synchronize helper
		/// </summary>
		private Action<Action> _syncHelper;

		/// <summary>
		///     The pk
		/// </summary>
		private string pk;

		/// <summary>
		///     The SQL version
		/// </summary>
		protected string SqlVersion;

		/// <summary>
		///     Typed list of all Elements
		/// </summary>
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
				{
					_syncHelper = value;
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MsSqlUntypedDataPager{T}" /> class.
		/// </summary>
		public MySqlUntypedDataPager()
		{
			CurrentPage = 1;
			PageSize = 10;
			AppendedComands = new List<IDbCommand>();
			CurrentPageItems = new ObservableCollection<T>();

			SyncHelper = action => action();
		}

		/// <summary>
		///     For Advanced querys including Order statements
		/// </summary>
		/// <value>
		///     The command query.
		/// </value>
		public IElementProducer<T> CommandQuery { get; set; }

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
		///     Base query to get a collection of <typeparamref name="T" /> Can NOT contain an Order Statement. Please use the
		///     CommandQuery property for this
		/// </summary>
		public IDbCommand BaseQuery { get; set; }

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
		///     Commands that are sequencely attached to the main pager command
		/// </summary>
		public List<IDbCommand> AppendedComands { get; set; }

		/// <summary>
		///     Id of Current page beween 1 and MaxPage
		/// </summary>
		/// <exception cref="InvalidOperationException">The current page must be bigger or equals 1</exception>
		public long CurrentPage
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
		public long MaxPage { get; private set; }

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
		///     Raises the new page loaded.
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
		///     Raises the new page loading.
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

		public virtual void LoadPage(DbAccessLayer dbAccess)
		{
			RaiseNewPageLoading();
			SyncHelper(CurrentPageItems.Clear);
			if (pk == null)
			{
				pk = typeof(T).GetPK(dbAccess.Config);
			}
			IDbCommand finalAppendCommand;

			if (CommandQuery != null)
			{
				TotalItemCount = new ElementProducer<T>(CommandQuery).CountInt().FirstOrDefault();
				MaxPage = (long) Math.Ceiling((decimal) TotalItemCount / PageSize);

				RaiseNewPageLoading();
				var elements =
					new ElementProducer<T>(CommandQuery).QueryD("LIMIT @PagedRows, @PageSize",
						new
						{
							PagedRows = (CurrentPage - 1) * PageSize,
							PageSize
						}).ToArray();

				foreach (var item in elements)
				{
					var item1 = item;
					SyncHelper(() => CurrentPageItems.Add(item1));
				}

				RaiseNewPageLoaded();
			}
			else
			{
				if (AppendedComands.Any())
				{
					if (BaseQuery == null)
					{
						BaseQuery = dbAccess.CreateSelect<T>();
					}

					finalAppendCommand = AppendedComands.Aggregate(BaseQuery,
						(current, comand) =>
							dbAccess.Database.MergeTextToParameters(current, comand, false, 1, true, false));
				}
				else
				{
					if (BaseQuery == null)
					{
						BaseQuery = dbAccess.CreateSelect<T>();
					}

					finalAppendCommand = BaseQuery;
				}
				var selectMaxCommand = dbAccess
					.Query()
					.QueryText("WITH CTE AS")
					.InBracket(query => query.QueryCommand(finalAppendCommand))
					.QueryText("SELECT COUNT(1) FROM CTE")
					.ContainerObject
					.Compile();

				////var selectMaxCommand = DbAccessLayerHelper.CreateCommand(s, "SELECT COUNT( * ) AS NR FROM " + TargetType.GetTableName());

				//if (finalAppendCommand != null)
				//    selectMaxCommand = DbAccessLayer.ConcatCommands(s, selectMaxCommand, finalAppendCommand);

				var maxItems = dbAccess.RunPrimetivSelect(typeof(long), selectMaxCommand).FirstOrDefault();
				if (maxItems != null)
				{
					long parsedCount;
					long.TryParse(maxItems.ToString(), out parsedCount);
					TotalItemCount = parsedCount;
					MaxPage = (long) Math.Ceiling((decimal) parsedCount / PageSize);
				}

				//Check select strategy
				//IF version is or higher then 11.0.2100.60 we can use OFFSET and FETCH
				//esle we need to do it the old way

				RaiseNewPageLoading();
				IDbCommand command;
				command = dbAccess
					.Query()
					.WithCte("CTE", cte => new SelectQuery<T>(cte.QueryCommand(finalAppendCommand)))
					.QueryText("SELECT * FROM")
					.QueryText("CTE")
					.QueryD("LIMIT @PagedRows, @PageSize;", new
					{
						PagedRows = (CurrentPage - 1) * PageSize,
						PageSize
					})
					.ContainerObject
					.Compile();

				var selectWhere = dbAccess.SelectNative(typeof(T), command, true).Cast<T>().ToArray();

				foreach (var item in selectWhere)
				{
					var item1 = item;
					SyncHelper(() => CurrentPageItems.Add(item1));
				}

				RaiseNewPageLoaded();
			}
		}

		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}