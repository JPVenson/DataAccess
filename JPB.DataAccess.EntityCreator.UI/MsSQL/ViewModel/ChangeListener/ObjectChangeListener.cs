using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.ErrorValidation;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.ChangeListener
{
    public class ObjectChangeListener<T, TE> : DataErrorBase<T, TE> where TE : class, IErrorInfoProvider<T>, new() where T : class
    {
        public ObjectChangeListener()
        {
            this.Init();
        }

        public ObjectChangeListener(Dispatcher dispatcher) : base(dispatcher)
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
