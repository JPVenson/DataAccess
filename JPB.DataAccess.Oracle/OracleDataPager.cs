using JPB.DataAccess.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Oracle
{
    public class OracleDataPager<T> : OracleUntypedDataPager<T>
    {
        public OracleDataPager()
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
