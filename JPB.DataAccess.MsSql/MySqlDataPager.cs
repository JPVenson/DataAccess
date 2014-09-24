using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MySql;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper.MsSql
{
    public class MySqlDataPager<T> : MySqlUntypedDataPager, IDataPager<T>
    {
        public MySqlDataPager()
            : base()
        {
            TargetType = typeof(T);
            CurrentPageItems = new ObservableCollection<T>();
        }

        public new ICollection<T> CurrentPageItems { get; private set; }

        public new void LoadPage(DbAccessLayer dbAccess)
        {
            base.LoadPage(dbAccess);
        }
    }
}