using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.MsSQLServerExplorer.ViewModel
{
    public class MainWindowViewModel : AsyncViewModelBase
    {
        public MainWindowViewModel()
        {
            ActiveServers = new ThreadSaveObservableCollection<ServerViewModel>();
            //ConnectCommand = new DelegateCommand(this.ExecuteConnect, this.CanExecuteConnect);
            base.SimpleWorkWithSyncContinue(GetActiveServers, s =>
            {
                foreach (var activeServer in s)
                {
                    ActiveServers.Add(new ServerViewModel(activeServer));
                }
            });
        }

        #region SelectedTable property

        private TableViewModel _selectedTable = default(TableViewModel);

        public TableViewModel SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                _selectedTable = value;
                SendPropertyChanged(() => SelectedTable);
            }
        }

        #endregion 

        public static IList<string> GetActiveServers()
        {
            var result = new Collection<string>();
            var instanceEnumerator = SqlDataSourceEnumerator.Instance;
            var instancesTable = instanceEnumerator.GetDataSources();
            foreach (DataRow row in instancesTable.Rows)
            {
                if (!string.IsNullOrEmpty(row["InstanceName"].ToString()))
                    result.Add(string.Format(@"{0}{1}", row["ServerName"], row["InstanceName"]));
                else
                    result.Add(row["ServerName"].ToString());
            }
            return result;
        }

        private ServerViewModel _serverName;

        public ServerViewModel SelectedServer
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                SendPropertyChanged(() => SelectedServer);
            }
        }

        private ThreadSaveObservableCollection<ServerViewModel> _activeServers;

        public ThreadSaveObservableCollection<ServerViewModel> ActiveServers
        {
            get { return _activeServers; }
            set
            {
                _activeServers = value;
                SendPropertyChanged(() => ActiveServers);
            }
        }

        //public DelegateCommand ConnectCommand { get; private set; }

        //public void ExecuteConnect(object sender)
        //{
        //    if (IsWorking)
        //        return;

        //    var serv = sender as ServerViewModel;
        //    Debug.Assert(serv != null, "serv != null");
        //    serv.EnumerateDatabases();
        //}

        //public bool CanExecuteConnect(object sender)
        //{
        //    return sender is ServerViewModel && ((ServerViewModel)sender).Databases == null;
        //}

        internal static SqlConnection GetActiveConnection(string serverName)
        {
            var connBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                IntegratedSecurity = true
            };
            return new SqlConnection(connBuilder.ConnectionString);
        }
    }
}
