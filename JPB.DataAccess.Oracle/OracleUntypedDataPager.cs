﻿using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Operators.Orders;

namespace JPB.DataAccess.Oracle
{
    public class OracleUntypedDataPager<T> : IDataPager<T>
    {
        private bool _cache;
        private int _currentPage;

        private Action<Action> _syncHelper;

        public OracleUntypedDataPager()
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
                {
                    throw new Exception("To be supported ... sory");
                }

                _cache = value;
            }
        }
        public long FirstID { get; private set; }
        public long LastID { get; private set; }

        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (value >= 0)
                {
                    _currentPage = value;
                }
            }
        }

        public virtual ICollection<T> CurrentPageItems { get; protected set; }
        public OrderByColumn<T> CommandQuery { get; set; }

        public int MaxPage { get; private set; }

        public int PageSize { get; set; }

        public Type TargetType { get; set; }

        public bool RaiseEvents { get; set; }

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

        public long TotalItemCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action NewPageLoaded;
        public event Action NewPageLoading;
        private void RaiseNewPageLoading()
        {
            var handler = NewPageLoading;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseNewPageLoaded()
        {
            var handler = NewPageLoaded;
            if (handler != null)
            {
                handler();
            }
        }

        public void Dispose()
        {
            CurrentPageItems.Clear();
        }

        public virtual void LoadPage(DbAccessLayer dbAccess)
        {
            throw new NotImplementedException();
        }
    }
}
