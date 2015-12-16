using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.MySql
{
    public class MySqlDataConverterPager<T, TE> : MySqlDataPager<T>, IWrapperDataPager<T,TE>
    {
        public MySqlDataConverterPager()
        {
            CurrentPageItems = new ObservableCollection<TE>();
        }
        
        public Func<T, TE> Converter { get; set; }
        public new ICollection<TE> CurrentPageItems { get; set; }

        public override void LoadPage(DbAccessLayer dbAccess)
        {
            base.LoadPage(dbAccess);

            CurrentPageItems.Clear();

            foreach (T currentPageItem in base.CurrentPageItems)
            {
                CurrentPageItems.Add(Converter(currentPageItem));
            }
        }
    }
}