using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.MsSQLServerExplorer.ViewModel
{
    public class ServerViewModel : AsyncViewModelBase
    {
        public ServerViewModel(string serverName)
        {
            ServerName = serverName;
            CreateTablesCommand = new DelegateCommand(CreateTables, CanCreateTables);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
        }

        #region Hide property

        private bool _hide = default(bool);

        public bool Hide
        {
            get { return _hide; }
            set
            {
                _hide = value;
                SendPropertyChanged(() => Hide);
            }
        }

        #endregion

        public static IList<string> GetDatabases(string serverName)
        {
            var result = new Collection<string>();
            using (var connection = MainWindowViewModel.GetActiveConnection(serverName))
            {
                try
                {
                    connection.Open();
                    var dt = connection.GetSchema(SqlClientMetaDataCollectionNames.Databases);
                    foreach (DataRow row in dt.Rows)
                    {
                        result.Add(string.Format("{0}", row[0]));
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return result;
        }

        #region Connect DelegateCommand

        public DelegateCommand ConnectCommand { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        private void Connect(object sender)
        {
            EnumerateDatabases();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        /// <returns>True if you can use it otherwise false</returns>
        private bool CanConnect(object sender)
        {
            return IsNotWorking && _databases == null;
        }

        #endregion 

        #region SelectedDatabase property

        private DatabaseViewModel _selectedDatabase = default(DatabaseViewModel);

        public DatabaseViewModel SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                _selectedDatabase = value;
                SendPropertyChanged(() => SelectedDatabase);
            }
        }

        #endregion

        #region CreateTables DelegateCommand

        public DelegateCommand CreateTablesCommand { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        private void CreateTables(object sender)
        {
            ((DatabaseViewModel) sender).EnumerateTables();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        /// <returns>True if you can use it otherwise false</returns>
        private bool CanCreateTables(object sender)
        {
            return SelectedDatabase != null && SelectedDatabase.Tables == null;
        }

        #endregion 

        private ThreadSaveObservableCollection<DatabaseViewModel> _databases;

        public ThreadSaveObservableCollection<DatabaseViewModel> Databases
        {
            get
            {
                return _databases;
            }
            set
            {
                _databases = value;
                SendPropertyChanged(() => Databases);
            }
        }

        public string ServerName { get; private set; }

        public void EnumerateDatabases()
        {
            if (_databases != null)
                return;

            _databases = new ThreadSaveObservableCollection<DatabaseViewModel>();
            base.SimpleWorkWithSyncContinue(() => GetDatabases(ServerName), s =>
            {
                if (s == null)
                {
                    Hide = true;
                    return;
                }

                foreach (var database in s)
                {
                    _databases.Add(new DatabaseViewModel(database, ServerName));
                }
            });
        }
    }
}