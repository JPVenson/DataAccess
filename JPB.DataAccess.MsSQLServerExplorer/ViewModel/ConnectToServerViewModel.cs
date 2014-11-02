using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.MsSQLServerExplorer.ViewModel
{
    public class ConnectToServerViewModel : AsyncViewModelBase
    {
        public ConnectToServerViewModel()
        {
            ActiveServers = new ThreadSaveObservableCollection<string>();

            base.SimpleWork(() =>
            {
                foreach (var activeServer in GetActiveServers())
                {
                    ActiveServers.Add(activeServer);
                }
            });
        }

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

        private ThreadSaveObservableCollection<string> _activeServers;

        public ThreadSaveObservableCollection<string> ActiveServers
        {
            get { return _activeServers; }
            set
            {
                _activeServers = value;
                SendPropertyChanged(() => ActiveServers);
            }
        }

        public DelegateCommand ConnectCommand { get; private set; }

        public void ExecuteConnect(object sender)
        {
            var conn = GetActiveConnection(ServerName);
            var successfully = false;
            try
            {
                conn.Open();
                successfully = true;
            }
            catch (Exception)
            {
                successfully = false;
            }

            if (successfully)
            {
                //open databases dialog
                MainWindowViewModel.Instance.SelectedDialog = new SelectDatabaseViewModel(ServerName);
            }
        }

        public bool CanExecuteConnect(object sender)
        {
            return !string.IsNullOrEmpty(ServerName);
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

    public class SelectDatabaseViewModel : AsyncViewModelBase
    {
        public SelectDatabaseViewModel(string serverName)
        {
            ServerName = serverName;
            Databases = new ThreadSaveObservableCollection<string>();
            SelectedTables = new ThreadSaveObservableCollection<string>();
            Tables = new ThreadSaveObservableCollection<string>();
            base.SimpleWork(() =>
            {
                foreach (var database in GetDatabases(ServerName))
                {
                    Databases.Add(database);
                }
            });
        }

        public static IList<string> GetDatabases(string serverName)
        {
            var result = new Collection<string>();
            using (var connection = ConnectToServerViewModel.GetActiveConnection(serverName))
            {
                connection.Open();
                DataTable dt = connection.GetSchema(SqlClientMetaDataCollectionNames.Databases);
                foreach (DataRow row in dt.Rows)
                {
                    result.Add(string.Format("{0}", row[0]));
                }
            }
            return result;
        }

        private ThreadSaveObservableCollection<string> _selectedTables;

        public ThreadSaveObservableCollection<string> SelectedTables
        {
            get { return _selectedTables; }
            set
            {
                _selectedTables = value;
                SendPropertyChanged(() => SelectedTables);
            }
        }

        private ThreadSaveObservableCollection<string> _databases;

        public ThreadSaveObservableCollection<string> Databases
        {
            get { return _databases; }
            set
            {
                _databases = value;
                SendPropertyChanged(() => Databases);
            }
        }

        private string _selectedDatabase;

        public string SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                _selectedDatabase = value;
                SendPropertyChanged(() => SelectedDatabase);
            }
        }

        private ThreadSaveObservableCollection<string> _tables;

        public ThreadSaveObservableCollection<string> Tables
        {
            get { return _tables; }
            set
            {
                _tables = value;
                SendPropertyChanged(() => Tables);
            }
        }

        public string ServerName { get; private set; }
    }
}
