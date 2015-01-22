using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Windows.Input;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Pager.Contracts
{
    /// <summary>
    /// Base pager
    /// </summary>
    public interface IDataPager
    {
        /// <summary>
        /// Not Implimented
        /// </summary>
        bool Cache { get; set; }

        /// <summary>
        /// Should raise Events
        /// </summary>
        bool RaiseEvents { get; set; }

        /// <summary>
        /// Raised if new Page is loading
        /// </summary>
        event Action NewPageLoading;

        /// <summary>
        /// Raised if new Page is loaded
        /// </summary>
        event Action NewPageLoaded;

        /// <summary>
        /// Commands that are sequencely attached to the main pager command
        /// </summary>
        List<IDbCommand> AppendedComands { get; set; }  

        /// <summary>
        /// Id of Current page beween 0 and MaxPage
        /// </summary>
        long CurrentPage { get; set; }
        /// <summary>
        /// The last possible Page
        /// </summary>
        long MaxPage { get; }

        /// <summary>
        /// Items to load on one page
        /// </summary>
        int PageSize { get; set; }
        
        /// <summary>
        /// Loads the PageSize into CurrentPageItems
        /// </summary>
        /// <param name="dbAccess"></param>
        void LoadPage(DbAccessLayer dbAccess);

        /// <summary>
        /// Additional Commands to append to the base load command
        /// </summary>
        IEnumerable CurrentPageItems { get; }

        /// <summary>
        /// 
        /// </summary>
        Action<Action> SyncHelper { get; set; }
    }

    /// <summary>
    /// Generic Data pager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataPager<T> : IDataPager
    {
        /// <summary>
        /// Additional Commands to append to the base load command
        /// </summary>
        new ICollection<T> CurrentPageItems { get; }
    }
}