using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JPB.DataAccess.Pager.Contracts
{
    public interface IUnGenericDataPager : IDataPager
    {
        ICollection<dynamic> CurrentPageItems { get; }

        Type TargetType { get; set; }
    }
}