using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.SqlServerExplorer.ComboboxViewModel
{
    public class MainWindowViewModel : AsyncViewModelBase
    {
        public MainWindowViewModel()
        {
            ActiveServers = new ThreadSaveObservableCollection<ServerViewModel>();
            ActiveDatabases = new ThreadSaveObservableCollection<DatabaseViewModel>();
            ActiveTables = new ThreadSaveObservableCollection<TableViewModel>();
            base.SimpleWorkWithSyncContinue(GetActiveServers, s =>
            {
                foreach (var activeServer in s)
                {
                    ActiveServers.Add(new ServerViewModel(activeServer));
                }
            });
        }

        private bool _canChangeServer;

        public bool CanChangeServer
        {
            get { return _canChangeServer; }
            set
            {
                _canChangeServer = value;
                SendPropertyChanged(() => CanChangeServer);
            }
        }

        private bool _canChangeTable;
        public bool CanChangeTable
        {
            get { return _canChangeTable; }
            set
            {
                _canChangeTable = value;
                SendPropertyChanged(() => CanChangeTable);
            }
        }

        private bool _canChangeDatabase;

        public bool CanChangeDatabase
        {
            get { return _canChangeDatabase; }
            set
            {
                _canChangeDatabase = value;
                SendPropertyChanged(() => CanChangeDatabase);
            }
        }

        private bool _isTableEnumerationInProgress;

        public bool IsTableEnumerationInProgress
        {
            get { return _isTableEnumerationInProgress; }
            set
            {
                _isTableEnumerationInProgress = value;
                SendPropertyChanged(() => IsTableEnumerationInProgress);
            }
        }

        private bool _isDatabaseEnumerationInProgress;
        public bool IsDatabaseEnumerationInProgress
        {
            get { return _isDatabaseEnumerationInProgress; }
            set
            {
                _isDatabaseEnumerationInProgress = value;
                SendPropertyChanged(() => IsDatabaseEnumerationInProgress);
            }
        }

        private bool _isServerEnumerationInProgress;

        public bool IsServerEnumerationInProgress
        {
            get { return _isServerEnumerationInProgress; }
            set
            {
                _isServerEnumerationInProgress = value;
                SendPropertyChanged(() => IsServerEnumerationInProgress);
            }
        }

        public IList<string> GetTables(string serverName, string databaseName)
        {
            var result = new Collection<string>();
            using (var connection = GetActiveConnection(serverName))
            {
                CanChangeServer = false;
                CanChangeDatabase = false;
                CanChangeTable = false;


                var restrictions = new string[4];
                restrictions[0] = databaseName; // database/catalog name   
                restrictions[1] = "dbo"; // owner/schema name   
                restrictions[2] = null; // table name   
                restrictions[3] = "BASE TABLE"; // table type    

                connection.Open();
                DataTable dt = connection.GetSchema(SqlClientMetaDataCollectionNames.Tables, restrictions);
                foreach (DataRow row in dt.Rows)
                {
                    if (!row[2].ToString().StartsWith("sys"))
                        result.Add(string.Format("{0}", row[2]));
                }
                CanChangeServer = true;
                CanChangeDatabase = true;
                CanChangeTable = true;

            }
            return result;
        }

        public IList<string> GetDatabases(string serverName)
        {
            var result = new Collection<string>();
            using (var connection = GetActiveConnection(serverName))
            {
                try
                {
                    CanChangeServer = false;
                    connection.Open();
                    var dt = connection.GetSchema(SqlClientMetaDataCollectionNames.Databases);
                    foreach (DataRow row in dt.Rows)
                    {
                        result.Add(String.Format("{0}", row[0]));
                    }
                }
                catch (Exception)
                {
                    return new string[0];
                }
                finally
                {
                    CanChangeServer = true;
                }
            }
            return result;
        }

        public IList<string> GetActiveServers()
        {
            var result = new Collection<string>();

            try
            {
                CanChangeServer = false;
                CanChangeDatabase = false;
                CanChangeTable = false;
                IsServerEnumerationInProgress = true;

                var instanceEnumerator = SqlDataSourceEnumerator.Instance;
                var instancesTable = instanceEnumerator.GetDataSources();
                foreach (DataRow row in instancesTable.Rows)
                {
                    if (!String.IsNullOrEmpty(row["InstanceName"].ToString()))
                        result.Add(String.Format(@"{0}{1}", row["ServerName"], row["InstanceName"]));
                    else
                        result.Add(row["ServerName"].ToString());
                }
            }
            finally
            {
                CanChangeServer = true;
                CanChangeDatabase = true;
                IsServerEnumerationInProgress = false;
            }
            return result;
        }

        private string _databaseName;
        private string _oldDatabaseName;
        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                ActiveTables.Clear();
                _databaseName = value;
                CanChangeTable = true;
                SendPropertyChanged(() => DatabaseName);
            }
        }

        private string _serverName;
        private string _oldServerName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                ActiveDatabases.Clear();
                ActiveTables.Clear();
                _serverName = value;
                SendPropertyChanged(() => ServerName);
            }
        }

        private string _tableName;

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                SendPropertyChanged(() => TableName);
            }
        }

        private ThreadSaveObservableCollection<TableViewModel> _activeTables;
        public ThreadSaveObservableCollection<TableViewModel> ActiveTables
        {
            get { return _activeTables; }
            set
            {
                _activeTables = value;
                SendPropertyChanged(() => ActiveTables);
            }
        }

        private ThreadSaveObservableCollection<DatabaseViewModel> _activeDatabases;
        public ThreadSaveObservableCollection<DatabaseViewModel> ActiveDatabases
        {
            get { return _activeDatabases; }
            set
            {
                _activeDatabases = value;
                SendPropertyChanged(() => ActiveDatabases);
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

        private bool _isTableOpen;

        public bool IsTableOpen
        {
            get { return _isTableOpen; }
            set
            {
                if (value && IsNotWorking && _oldDatabaseName != DatabaseName)
                {
                    _oldDatabaseName = DatabaseName;
                    base.SimpleWork(() =>
                    {
                        IsTableEnumerationInProgress = true;
                        ActiveTables.Clear();
                        var tables = GetTables(ServerName, DatabaseName);
                        foreach (var table in tables)
                        {
                            ActiveTables.Add(new TableViewModel(table));
                        }
                        IsTableEnumerationInProgress = false;
                    });
                }

                _isTableOpen = value;
                SendPropertyChanged(() => IsTableOpen);
            }
        }

        private bool _isDatabaseOpen;
        public bool IsDatabaseOpen
        {
            get { return _isDatabaseOpen; }
            set
            {
                if (value && IsNotWorking && _oldServerName != ServerName)
                {
                    _oldServerName = ServerName;
                    base.SimpleWork(() =>
                    {
                        IsDatabaseEnumerationInProgress = true;
                        ActiveDatabases.Clear();
                        var databases = GetDatabases(ServerName);
                        foreach (var databaseViewModel in databases)
                        {
                            ActiveDatabases.Add(new DatabaseViewModel(databaseViewModel, ServerName));
                        }
                        IsDatabaseEnumerationInProgress = false;
                    });
                }
                _isDatabaseOpen = value;
                SendPropertyChanged(() => IsDatabaseOpen);
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
