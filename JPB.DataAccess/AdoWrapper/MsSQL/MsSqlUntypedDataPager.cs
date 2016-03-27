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
using JPB.DataAccess.QueryBuilder;

namespace JPB.DataAccess.AdoWrapper.MsSql
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MsSqlUntypedDataPager<T> : IDataPager<T>
	{
		protected string SqlVersion;
		private bool _cache;
		private bool? _checkRun;
		private long _currentPage;
		private Action<Action> _syncHelper;

		/// <summary>
		/// </summary>
		public MsSqlUntypedDataPager()
		{
			CurrentPage = 0;
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
				if (value >= 0)
					_currentPage = value;
			}
		}

		public long MaxPage { get; private set; }

		public int PageSize { get; set; }

		public ICollection<T> CurrentPageItems { get; protected set; }

		void IDataPager.LoadPage(DbAccessLayer dbAccess)
		{
			T[] selectWhere = null;
			dbAccess.Database.RunInTransaction(s =>
			{
				IDbCommand finalAppendCommand;
				if (AppendedComands.Any())
				{
					finalAppendCommand = AppendedComands.Aggregate(DbAccessLayerHelper.CreateCommand(s, "WHERE"),
						(current, comand) => DbAccessLayerHelper.ConcatCommands(s, current, comand));
				}
				else
				{
					if (BaseQuery == null)
					{
						BaseQuery = dbAccess.CreateSelect<T>(dbAccess.Database);
					}

					finalAppendCommand = BaseQuery;
				}

				if (string.IsNullOrEmpty(SqlVersion))
				{
					SqlVersion = dbAccess.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
				}

				SyncHelper(CurrentPageItems.Clear);

				var pk = TargetType.GetPK();

				var selectMaxCommand = dbAccess
					.Query()
					.QueryText("WITH CTE AS")
					.InBracket(query => query.QueryCommand(finalAppendCommand))
					.LineBreak()
					.QueryText("SELECT COUNT(1) FROM CTE")
					.ContainerObject
					.Compile();

				////var selectMaxCommand = DbAccessLayerHelper.CreateCommand(s, "SELECT COUNT( * ) AS NR FROM " + TargetType.GetTableName());

				//if (finalAppendCommand != null)
				//    selectMaxCommand = DbAccessLayer.ConcatCommands(s, selectMaxCommand, finalAppendCommand);

				var maxItems = dbAccess.RunPrimetivSelect(typeof (long), selectMaxCommand).FirstOrDefault();
				if (maxItems != null)
				{
					long parsedCount;
					long.TryParse(maxItems.ToString(), out parsedCount);
					MaxPage = parsedCount/PageSize;
				}

				//Check select strategy
				//IF version is or higher then 11.0.2100.60 we can use OFFSET and FETCH
				//esle we need to do it the old way

				RaiseNewPageLoading();
				IDbCommand command;

				if (CheckVersionForFetch())
				{
					command = dbAccess.Query()
						.WithCte("CTE", cte => cte.QueryCommand(finalAppendCommand))
						.SelectStar()
						.QueryText("CTE")
						.QueryText("ORDER BY")
						.QueryD(pk)
						.QueryD("ASC OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY", new
						{
							PagedRows = CurrentPage*PageSize,
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
								baseCte.Select(typeof (T));
							}
							else
							{
								baseCte.QueryCommand(finalAppendCommand);
							}
						})
						.WithCte("CTE", cte =>
						{
							cte.SelectStarFrom(sel =>
							{
								sel.Select()
									.RowNumberOrder("@pk")
									.WithParamerters(new {Pk = pk})
									.As("RowNr")
									.QueryText(", BASECTE.* FROM BASECTE");
							})
								.As("TBL")
								.Where("RowNr")
								.Between(page =>
								{
									page.QueryText("@PagedRows * @PageSize + 1")
										.WithParamerters(new
										{
											PagedRows = CurrentPage,
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
				//cannot cast to T[] 
				selectWhere = dbAccess.SelectNative(TargetType, s, command, true).Cast<T>().ToArray();
			});

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

		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}