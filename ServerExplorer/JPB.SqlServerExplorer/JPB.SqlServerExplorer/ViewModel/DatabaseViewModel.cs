using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.SqlServerExplorer.ViewModel
{
    public class DatabaseViewModel : AsyncViewModelBase
    {
        public string Database { get; set; }
        public string ServerName { get; set; }

        public DatabaseViewModel(string database, string serverName)
        {
            Database = database;
            ServerName = serverName;
            EnumerateTablesCommand = new DelegateCommand(EnumerateTables, CanEnumerateTables);
        }

        public static IList<string> GetTables(string serverName, string databaseName)
        {
            var result = new Collection<string>();
            using (var connection = MainWindowViewModel.GetActiveConnection(serverName))
            {
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
            }
            return result;
        }

        private ThreadSaveObservableCollection<TableViewModel> _tables;

        public ThreadSaveObservableCollection<TableViewModel> Tables
        {
            get
            {
                return _tables;
            }
            set
            {
                _tables = value;
                SendPropertyChanged(() => Tables);
            }
        }

        #region EnumerateTables DelegateCommand

        public DelegateCommand EnumerateTablesCommand { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        private void EnumerateTables(object sender)
        {
            EnumerateTables();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender">The transferparameter</param>
        /// <returns>True if you can use it otherwise false</returns>
        private bool CanEnumerateTables(object sender)
        {
            return _tables == null && IsNotWorking;
        }

        #endregion 

        public void EnumerateTables()
        {
            if (_tables != null)
                return;

            _tables = new ThreadSaveObservableCollection<TableViewModel>();
            base.SimpleWorkWithSyncContinue(() =>
            {
                _tables.Clear();
                return GetTables(ServerName, Database);
            }, s =>
            {
                foreach (var table in s)
                {
                    _tables.Add(new TableViewModel(table));
                }
            });
        }
    }
}