using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.DataAccess.Manager;

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
