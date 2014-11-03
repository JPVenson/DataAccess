using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.SqlServerExplorer.ComboboxViewModel
{
    public class DatabaseViewModel : AsyncViewModelBase
    {
        public string Database { get; set; }
        public string ServerName { get; set; }

        public DatabaseViewModel(string database, string serverName)
        {
            Database = database;
            ServerName = serverName;
        }

        //public void EnumerateTables()
        //{
        //    if (_tables != null)
        //        return;

        //    _tables = new ThreadSaveObservableCollection<TableViewModel>();
        //    base.SimpleWorkWithSyncContinue(() =>
        //    {
        //        _tables.Clear();
        //        return GetTables(ServerName, Database);
        //    }, s =>
        //    {
        //        foreach (var table in s)
        //        {
        //            _tables.Add(new TableViewModel(table));
        //        }
        //    });
        //}
    }
}