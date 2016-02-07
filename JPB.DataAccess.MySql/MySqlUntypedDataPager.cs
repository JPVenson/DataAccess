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
using System.Data.Common;
using System.Linq;
using System.Text;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;
using MySql.Data.MySqlClient;

namespace JPB.DataAccess.MySql
{
	public class MySqlUntypedDataPager<T> : IDataPager<T>
	{
		public MySqlUntypedDataPager()
		{
			CurrentPage = 0;
			PageSize = 10;

			FirstID = -1;
			LastID = -1;
			SyncHelper = action => action();
		}

		public bool Cache
		{
			get { return _cache; }
			set
			{
				if (value)
					throw new Exception("To be supported ... sory");
				_cache = value;
			}
		}

		public bool RaiseEvents { get; set; }
		public event Action NewPageLoading;
		public event Action NewPageLoaded;
		public List<IDbCommand> AppendedComands { get; set; }

		public long FirstID { get; private set; }
		public long LastID { get; private set; }

		private void RaiseNewPageLoading() { var handler = NewPageLoading; if (handler != null) handler(); }
		private void RaiseNewPageLoaded() { var handler = NewPageLoaded; if (handler != null) handler(); }

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

		public Type TargetType { get; set; }

		public virtual void LoadPage(DbAccessLayer dbAccess)
		{
			RaiseNewPageLoading();
			SyncHelper(() => CurrentPageItems.Clear());

			var pk = TargetType.GetPK();

			var targetQuery = BaseQuery;
			if (targetQuery == null)
			{
				targetQuery = dbAccess.Database.CreateCommand(TargetType.GetClassInfo().TableName);
			}

			IDbCommand FirstIdCommand = targetQuery;
			if (AppendedComands.Any())
			{
				FirstIdCommand = this.AppendedComands.Aggregate(FirstIdCommand,
					(e, f) => DbAccessLayer.ConcatCommands(dbAccess.Database, e, f));
			}


			if (FirstID == -1 || LastID == -1)
			{

				var firstOrDefault = dbAccess.RunPrimetivSelect(typeof(long),
					DbAccessLayer.InsertCommands(dbAccess.Database,
						("SELECT " + pk + " FROM ( {0} ) ORDER BY " + pk + " LIMIT 1").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();
				if (firstOrDefault != null)
					FirstID = (long)firstOrDefault;

				var lastId = dbAccess.RunPrimetivSelect(typeof(long),
					DbAccessLayer.InsertCommands(dbAccess.Database,
					("SELECT " + pk + " FROM ( {0} ) ORDER BY " + pk + " DESC LIMIT 1").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();
				if (lastId != null)
					LastID = (long)lastId;
			}

			var maxItems = dbAccess.RunPrimetivSelect(typeof(long),
	DbAccessLayer.InsertCommands(dbAccess.Database,
	("SELECT COUNT( * ) AS NR FROM {0}").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();

			if (maxItems != null)
			{
				long parsedCount;
				long.TryParse(maxItems.ToString(), out parsedCount);
				MaxPage = ((long)parsedCount) / PageSize;
			}

			var realSelect = DbAccessLayer.InsertCommands(dbAccess.Database,
				("SELECT * FROM {0}").CreateCommand(dbAccess.Database), FirstIdCommand);

			var selectWhere = dbAccess.SelectNative(this.TargetType, realSelect, new
			{
				PagedRows = CurrentPage * PageSize,
				PageSize
			});

			//var selectWhere = dbAccess.SelectWhere(TargetType, " ORDER BY " + pk + " ASC LIMIT @PagedRows, @PageSize", new
			//{
			//    PagedRows = CurrentPage * PageSize,
			//    PageSize
			//});

			foreach (var item in selectWhere)
			{
				dynamic item1 = item;
				SyncHelper(() => CurrentPageItems.Add(item1));
			}

			RaiseNewPageLoaded();
		}

		IEnumerable IDataPager.CurrentPageItems
		{
			get { return this.CurrentPageItems; }
		}

		public IDbCommand BaseQuery { get; set; }

		public virtual ICollection<T> CurrentPageItems { get; protected set; }

		private Action<Action> _syncHelper;
		private long _currentPage;
		private bool _cache;

		public Action<Action> SyncHelper
		{
			get { return _syncHelper; }
			set
			{
				if (value != null)
					_syncHelper = value;
			}
		}

		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}