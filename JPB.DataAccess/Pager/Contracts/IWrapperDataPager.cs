using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JPB.DataAccess.Pager.Contracts
{
    public interface IWrapperDataPager<T,TE> : IUnGenericDataPager
    {
        Func<T,TE> Converter { get; set; }
        new ICollection<TE> CurrentPageItems { get; set; }
    }
}