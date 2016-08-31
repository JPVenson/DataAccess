/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.MySql
{
    public class MySqlDataPager<T> : MySqlUntypedDataPager<T>
    {
        public MySqlDataPager()
            : base()
        {
            TargetType = typeof(T);
            CurrentPageItems = new ObservableCollection<T>();
        }

        public override ICollection<T> CurrentPageItems { get; protected set; }

        public override void LoadPage(DbAccessLayer dbAccess)
        {
            base.LoadPage(dbAccess);
        }
    }
}