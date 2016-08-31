/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

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