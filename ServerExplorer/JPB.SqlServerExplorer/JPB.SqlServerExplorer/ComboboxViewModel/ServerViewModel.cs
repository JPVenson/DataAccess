using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.SqlServerExplorer.ComboboxViewModel
{
    public class ServerViewModel : AsyncViewModelBase
    {
        public ServerViewModel(string serverName)
        {
            ServerName = serverName;
        }

        public string ServerName { get; private set; }
    }
}