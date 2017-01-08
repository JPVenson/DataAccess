﻿/*
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
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.Operators.Orders;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.Pager.IDataPager{T}" />
	public class MsSqlUntypedDataPager<T> : IDataPager<T>
	{
		/// <summary>
		/// The SQL version
		/// </summary>
		protected string SqlVersion;
		/// <summary>
		/// The cache
		/// </summary>
		private bool _cache;
		/// <summary>
		/// The check run
		/// </summary>
		private bool? _checkRun;
		/// <summary>
		/// The current page
		/// </summary>
		private long _currentPage;
		/// <summary>
		/// The synchronize helper
		/// </summary>
		private Action<Action> _syncHelper;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsSqlUntypedDataPager{T}"/> class.
		/// </summary>
		public MsSqlUntypedDataPager()
		{
			CurrentPage = 1;
			PageSize = 10;
			AppendedComands = new List<IDbCommand>();
			CurrentPageItems = new ObservableCollection<T>();

			SyncHelper = action => action();
		}

		/// <summary>
		/// Not Implimented
		/// </summary>
		/// <exception cref="Exception">To be supported ... sory</exception>
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

		/// <summary>
		/// Base query to get a collection of <typeparamref name="T" /> Can NOT contain an Order Statement. Please use the CommandQuery property for this
		/// </summary>
		public IDbCommand BaseQuery { get; set; }

		/// <summary>
		/// For Advanced querys including Order statements
		/// </summary>
		/// <value>
		/// The command query.
		/// </value>
		public ElementProducer<T> CommandQuery { get; set; }

		/// <summary>
		/// Should raise Events
		/// </summary>
		public bool RaiseEvents { get; set; }

		/// <summary>
		/// Raised if new Page is loading
		/// </summary>
		public event Action NewPageLoading;

		/// <summary>
		/// Raised if new page is Loaded
		/// </summary>
		public event Action NewPageLoaded;

		/// <summary>
		/// Commands that are sequencely attached to the main pager command
		/// </summary>
		public List<IDbCommand> AppendedComands { get; set; }

		/// <summary>
		/// Id of Current page beween 1 and MaxPage
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
		/// The pk
		/// </summary>
		string pk;

		/// <summary>
		/// The last possible Page
		/// </summary>
		public long MaxPage { get; private set; }

		/// <summary>
		/// Items to load on one page
		/// </summary>
		public int PageSize { get; set; }
		/// <summary>
		/// Get the complete ammount of all items listend
		/// </summary>
		public long TotalItemCount { get; private set; }

		/// <summary>
		/// Typed list of all Elements
		/// </summary>
		public ICollection<T> CurrentPageItems { get; protected set; }

		/// <summary>
		/// Loads the PageSize into CurrentPageItems
		/// </summary>
		/// <param name="dbAccess"></param>
		void IDataPager.LoadPage(DbAccessLayer dbAccess)
		{
			T[] selectWhere = null;
			IDbCommand finalAppendCommand;

			if (string.IsNullOrEmpty(SqlVersion))
			{
				SqlVersion = dbAccess.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
			}

			SyncHelper(CurrentPageItems.Clear);
			if (pk == null)
				pk = typeof(T).GetPK(dbAccess.Config);

			if (CommandQuery != null)
			{
				TotalItemCount = new ElementProducer<T>(this.CommandQuery.Clone()).CountInt().FirstOrDefault();
				MaxPage = (long)Math.Ceiling((decimal)TotalItemCount / PageSize);

				RaiseNewPageLoading();
				var elements = new ElementProducer<T>(this.CommandQuery.Clone()).QueryD("OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY", new
				{
					PagedRows = (CurrentPage - 1) * PageSize,
					PageSize
				}).ToArray();

				foreach (T item in elements)
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
					MaxPage = (long)Math.Ceiling((decimal)parsedCount / PageSize);
				}

				//Check select strategy
				//IF version is or higher then 11.0.2100.60 we can use OFFSET and FETCH
				//esle we need to do it the old way

				RaiseNewPageLoading();
				IDbCommand command;

				if (CheckVersionForFetch())
				{
					command = dbAccess
						.Query()
						.WithCte("CTE", cte => cte.QueryCommand(finalAppendCommand))
						.QueryText("SELECT * FROM")
						.QueryText("CTE")
						.QueryText("ORDER BY")
						.QueryD(pk)
						.QueryD("ASC OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY", new
						{
							PagedRows = (CurrentPage - 1) * PageSize,
							PageSize
						})
						.ContainerObject
						.Compile();
				}
				else
				{
					// ReSharper disable ConvertToLambdaExpression
					var selectQuery = dbAccess.Query()
						.WithCte("BASECTE", baseCte =>
						{
							if (BaseQuery != null)
							{
								baseCte.Select<T>();
							}
							else
							{
								baseCte.QueryCommand(finalAppendCommand);
							}
						})
						.WithCte("CTE", cte =>
						{
							cte.QueryText("SELECT * FROM (")
									.Select<T>()
									.RowNumberOrder("@pk")
									.WithParamerters(new { Pk = pk })
									.QueryText("AS RowNr")
									.QueryText(", BASECTE.* FROM BASECTE")
								.QueryText(")")
								.As("TBL")
								.Where()
								.Column("RowNr")
								.Is()
								.Between(page =>
								{
									page.QueryText("@PagedRows * @PageSize + 1")
										.WithParamerters(new
										{
											PagedRows = (CurrentPage - 1),
											PageSize
										});
								},
									maxPage =>
									{
										maxPage
											.InBracket(calc => { calc.QueryText("@PagedRows + 1"); })
											.QueryText("* @PageSize");
									}
								);
						}, true)
						.QueryText("SELECT * FROM CTE");

					command = selectQuery.ContainerObject.Compile();
				}
				selectWhere = dbAccess.SelectNative(typeof(T), command, true).Cast<T>().ToArray();

				foreach (T item in selectWhere)
				{
					var item1 = item;
					SyncHelper(() => CurrentPageItems.Add(item1));
				}

				RaiseNewPageLoaded();
			}
		}

		/// <summary>
		/// Typed list of all Elements
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
					_syncHelper = value;
			}
		}

		/// <summary>
		/// Raises the new page loaded.
		/// </summary>
		protected virtual void RaiseNewPageLoaded()
		{
			if (!RaiseEvents)
				return;
			var handler = NewPageLoaded;
			if (handler != null) handler();
		}

		/// <summary>
		/// Raises the new page loading.
		/// </summary>
		protected virtual void RaiseNewPageLoading()
		{
			if (!RaiseEvents)
				return;
			var handler = NewPageLoading;
			if (handler != null) handler();
		}

		/// <summary>
		/// Checks the version for fetch.
		/// </summary>
		/// <returns></returns>
		private bool CheckVersionForFetch()
		{
			if (_checkRun != null)
				return _checkRun.Value;

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

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}