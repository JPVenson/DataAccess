using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.MySql
{
    public class MySqlDataPager<T> : MySqlUntypedDataPager<T>, IDataPager<T>
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