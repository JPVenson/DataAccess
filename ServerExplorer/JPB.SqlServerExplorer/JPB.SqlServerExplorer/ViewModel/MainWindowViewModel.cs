using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.SqlServerExplorer.ViewModel
{
    public class MainWindowViewModel : AsyncViewModelBase
    {
        public MainWindowViewModel()
        {
            ActiveServers = new ThreadSaveObservableCollection<ServerViewModel>();
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

        private ServerViewModel _selectedServer;

        public ServerViewModel SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                ServerName = value.ServerName;
                SendPropertyChanged(() => SelectedServer);
            }
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                SendPropertyChanged(() => ServerName);
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
