using System;
using System.Collections.ObjectModel;

namespace JPB.DataAccess.Pager.Contracts
{
    public interface IUnGenericDataPager : IDataPager
    {
        ObservableCollection<dynamic> CurrentPageItems { get; }

        Type TargetType { get; set; }
    }
}