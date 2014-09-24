using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Pager.Contracts
{
    public interface IDataPager<T> : IUnGenericDataPager
    {
        new ICollection<T> CurrentPageItems { get; }
    }

    public interface IDataPager
    {
        bool Cache { get; set; }

        long FirstID { get; }
        long LastID { get; }
        long CurrentPage { get; set; }
        long MaxPage { get; }

        int PageSize { get; set; }
        
        void LoadPage(DbAccessLayer dbAccess);

        Action<Action> SyncHelper { get; set; }
    }
}