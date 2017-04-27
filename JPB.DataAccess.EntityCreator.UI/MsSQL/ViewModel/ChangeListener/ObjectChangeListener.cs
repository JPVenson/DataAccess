using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ViewModelProvider;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.ChangeListener
{
    public class ObjectChangeListener<T, TE> : AsyncErrorProviderBase<TE> where TE : class, IErrorCollectionBase, new()
    {
        public ObjectChangeListener()
        {
            this.Init();
        }

        private void Init()
        {
            base.PropertyChanged += ObjectChangeListener_PropertyChanged;
        }

        private void ObjectChangeListener_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}
