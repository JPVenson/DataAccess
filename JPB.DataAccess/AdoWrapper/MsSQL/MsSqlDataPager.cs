using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Pager.Contracts;

namespace JPB.DataAccess.AdoWrapper.MsSql
{
    public class MsSqlDataPager<T> : MsSqlUntypedDataPager<T>
    {
        static MsSqlDataPager()
        {
            _type = typeof(T);
        }

        public MsSqlDataPager()
            : base()
        {
            TargetType = _type;
        }
        
        private static Type _type;

        public new ICollection<T> CurrentPageItems
        {
            get { return base.CurrentPageItems; }
        }
    }
}