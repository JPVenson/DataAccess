using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Oracle
{
    class OracleDataConverterPager<T, TE> : OracleDataPager<T>, IWrapperDataPager<T, TE>
    {
        public OracleDataConverterPager()
        {
            CurrentPageItems = new ObservableCollection<TE>();
        }

        public Func<T, TE> Converter { get; set; }
        public new ICollection<TE> CurrentPageItems { get; set; }

        ICollection<TE> IWrapperDataPager<T, TE>.CurrentPageItems
        {
            get
            {
                throw new NotImplementedException();
            }
        }
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
