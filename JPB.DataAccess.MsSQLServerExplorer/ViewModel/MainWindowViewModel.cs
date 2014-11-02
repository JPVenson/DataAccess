using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Win32;
using WpfApplication1;

namespace JPB.DataAccess.MsSQLServerExplorer.ViewModel
{
    public class MainWindowViewModel : AsyncViewModelBase
    {
        public MainWindowViewModel()
        {
            Instance = this;

            SelectedDialog = new ConnectToServerViewModel();
        }

        public static MainWindowViewModel Instance;

        private object _selectedDialog;

        public object SelectedDialog
        {
            get { return _selectedDialog; }
            set
            {
                _selectedDialog = value;
                SendPropertyChanged(() => SelectedDialog);
            }
        }
    }
}
