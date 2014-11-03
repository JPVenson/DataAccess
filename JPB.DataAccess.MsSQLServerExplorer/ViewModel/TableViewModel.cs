using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.MsSQLServerExplorer.ViewModel
{
    public class TableViewModel : AsyncViewModelBase
    {
        public TableViewModel(string table)
        {
            TableName = table;
        }

        #region TableName property

        private string _tableName = default(string);

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                SendPropertyChanged(() => TableName);
            }
        }

        #endregion

        #region IsSelected property

        private bool _isSelected = default(bool);

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (value)
                {
                    TableSelector.Instance.Add(this);
                }
                else
                {
                    TableSelector.Instance.Remove(this);
                }
                SendPropertyChanged(() => IsSelected);
            }
        }

        #endregion 
    }

    public class TableSelector : ThreadSaveObservableCollection<TableViewModel>
    {
        static TableSelector()
        {
            Instance = new TableSelector();
        }

        private TableSelector()
        {

        }

        public static TableSelector Instance { get; set; } 
    }
}