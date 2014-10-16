using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.MySql
{
    public class MySqlUntypedDataPager : IDataPager
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

        public void LoadPage(DbAccessLayer dbAccess)
        {
            //SyncHelper(CurrentPageItems.Clear);

            //var pk = TargetType.GetPK();

            //if (FirstID == -1 || LastID == -1)
            //{
            //    var firstOrDefault = dbAccess.RunPrimetivSelect(typeof(long), "SELECT " + pk + " FROM " + TargetType.GetTableName() + " ORDER BY " + pk + " LIMIT 1").FirstOrDefault();
            //    if (firstOrDefault != null)
            //        FirstID = (long)firstOrDefault;

            //    var lastId = dbAccess.RunPrimetivSelect(typeof(long), "SELECT " + pk + " FROM " + TargetType.GetTableName() + " ORDER BY " + pk + " DESC LIMIT 1").FirstOrDefault();
            //    if (lastId != null)
            //        LastID = (long)lastId;
            //}

            //var maxItems = dbAccess.RunPrimetivSelect(typeof(long), "SELECT COUNT( * ) AS NR FROM " + TargetType.GetTableName()).FirstOrDefault();
            //if (maxItems != null)
            //{
            //    long parsedCount;
            //    long.TryParse(maxItems.ToString(), out parsedCount);
            //    MaxPage = ((long)parsedCount) / PageSize;
            //}

            //var selectWhere = dbAccess.SelectWhere(TargetType, " ORDER BY " + pk + " ASC LIMIT @PagedRows, @PageSize", new
            //{
            //    PagedRows = CurrentPage * PageSize,
            //    PageSize
            //});

            //foreach (var item in selectWhere)
            //{
            //    dynamic item1 = item;
            //    SyncHelper(() => CurrentPageItems.Add(item1));
            //}
        }

        public ICollection CurrentPageItems { get; private set; }

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
    }
}