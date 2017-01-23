using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.EntityCreator.Core;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.Manager;
using JPB.DynamicInputBox;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Data.ConnectionUI;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
    public class SqlEntityCreatorViewModel : AsyncViewModelBase, IMsSqlCreator
    {
        public SqlEntityCreatorViewModel()
        {
            AdjustNamesCommand = new DelegateCommand(AdjustNamesExecute, CanAdjustNamesExecute);
            CompileCommand = new DelegateCommand(CompileExecute, CanCompileExecute);
            ConnectToDatabaseCommand = new DelegateCommand(ConnectToDatabaseExecute, CanConnectToDatabaseExecute); SaveConfigCommand = new DelegateCommand(SaveConfigExecute, CanSaveConfigExecute);
            LoadConfigCommand = new DelegateCommand(LoadConfigExecute, CanLoadConfigExecute);
            OpenInfoWindowCommand = new DelegateCommand(OpenInfoWindowExecute);
            DeleteSelectedTableCommand = new DelegateCommand(DeleteSelectedTableExecute, CanDeleteSelectedTableExecute);
            AddTableCommand = new DelegateCommand(AddTableExecute, CanAddTableExecute);

            Tables = new ThreadSaveObservableCollection<TableInfoViewModel>();
            Views = new ThreadSaveObservableCollection<TableInfoViewModel>();
            StoredProcs = new ThreadSaveObservableCollection<IStoredPrcInfoModel>();
            Enums = new ThreadSaveObservableCollection<Dictionary<int, string>>();

            SharedMethods.Logger = new DelegateLogger((message) => this.Status = message);
        }

        public DelegateCommand OpenInfoWindowCommand { get; set; }

        public void OpenInfoWindowExecute(object sender)
        {
            new InfoWindow().ShowDialog();
        }

        public DelegateCommand LoadConfigCommand { get; private set; }

        private void LoadConfigExecute(object sender)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.CheckFileExists = true;
            fileDialog.DefaultExt = "*.msConfigStore";
            fileDialog.Filter = "ConfigFile (*.msConfigStore)|*.msConfigStore";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value == true && File.Exists(fileDialog.FileName))
            {
                this.Tables.Clear();
                this.Views.Clear();
                this.StoredProcs.Clear();

                var binFormatter = new BinaryFormatter();
                ConfigStore options;
                try
                {
                    using (var fs = fileDialog.OpenFile())
                    {
                        options = (ConfigStore)binFormatter.Deserialize(fs);
                    }
                }
                catch (Exception)
                {
                    Status = "File is an in invalid format";
                    return;
                }

                var version = typeof(SharedMethods).Assembly.GetName().Version;
                if (new Version(options.Version) != version)
                {
                    var messageBoxResult = MessageBox.Show(App.Current.MainWindow,
                        "Warning Version missmatch",
                        string.Format("The current Entity Creator version ({0}) is not equals the version ({1}) you have provided.",
                            version, options.Version),
                        MessageBoxButton.OKCancel);

                    if (messageBoxResult == MessageBoxResult.Cancel)
                        return;
                }

                foreach (var option in options.Tables)
                {
                    var itemExisits = this.Tables.FirstOrDefault(s => s.Info.TableName == option.Info.TableName);
                    if (itemExisits != null)
                    {
                        this.Tables.Remove(itemExisits);
                    }

                    this.Tables.Add(new TableInfoViewModel(option, this));
                }
                foreach (var option in options.Views)
                {
                    this.Views.Add(new TableInfoViewModel(option, this));
                }
                foreach (var option in options.StoredPrcInfoModels)
                {
                    this.StoredProcs.Add(option);
                }

                if (options.SourceConnectionString != null)
                {
                    this.ConnectionString = options.SourceConnectionString;
                    this.CreateEntrys(this.ConnectionString, "", string.Empty);
                }

                this.GenerateConstructor = options.GenerateConstructor;
                this.GenerateForgeinKeyDeclarations = options.GenerateForgeinKeyDeclarations;
                this.GenerateCompilerHeader = options.GenerateCompilerHeader;
                this.GenerateConfigMethod = options.GenerateConfigMethod;
                this.Namespace = options.Namespace;
                this.TargetDir = options.TargetDir;
                this.SelectedTable = Tables.FirstOrDefault();
            }
        }

        private bool CanLoadConfigExecute(object sender)
        {
            return true;
        }

        public DelegateCommand SaveConfigCommand { get; private set; }

        private void SaveConfigExecute(object sender)
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.DefaultExt = ".msConfigStore";
            fileDialog.Filter = "ConfigFile (*.msConfigStore)|*.msConfigStore";
            fileDialog.CheckFileExists = false;
            var fileResult = fileDialog.ShowDialog();
            if (fileResult == DialogResult.OK)
            {
                var options = new ConfigStore();
                options.StoredPrcInfoModels = this.StoredProcs.ToList();
                options.Views = this.Views.Select(s => s.SourceElement).ToList();
                options.Tables = this.Tables.Select(s => s.SourceElement).ToList();

                options.GenerateConstructor = this.GenerateConstructor;
                options.GenerateForgeinKeyDeclarations = this.GenerateForgeinKeyDeclarations;
                options.GenerateCompilerHeader = this.GenerateCompilerHeader;
                options.GenerateConfigMethod = this.GenerateConfigMethod;
                options.Namespace = this.Namespace;
                options.TargetDir = this.TargetDir;

                var version = typeof(SharedMethods).Assembly.GetName().Version;

                options.Version = version.ToString();

                if (this.ConnectionString != null)
                {
                    options.SourceConnectionString = this.ConnectionString;
                }

                if (File.Exists(fileDialog.FileName))
                {
                    File.Delete(fileDialog.FileName);
                }
                using (var fs = fileDialog.OpenFile())
                {
                    new BinaryFormatter().Serialize(fs, options);
                }
            }
        }

        private bool CanSaveConfigExecute(object sender)
        {
            return true;
        }

        private TableInfoViewModel _selectedTable;

        public TableInfoViewModel SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                SendPropertyChanging(() => SelectedTable);
                _selectedTable = value;
                SendPropertyChanged(() => SelectedTable);
            }
        }

        private ThreadSaveObservableCollection<TableInfoViewModel> _tables;

        public ThreadSaveObservableCollection<TableInfoViewModel> Tables
        {
            get { return _tables; }
            set
            {
                SendPropertyChanging(() => Tables);
                _tables = value;
                SendPropertyChanged(() => Tables);
            }
        }

        private ThreadSaveObservableCollection<Dictionary<int, string>> _enums;

        IEnumerable<ITableInfoModel> IMsSqlCreator.Tables
        {
            get { return Tables; }
            set { }
        }

        public ThreadSaveObservableCollection<Dictionary<int, string>> Enums
        {
            get { return _enums; }
            set
            {
                SendPropertyChanging(() => Enums);
                _enums = value;
                SendPropertyChanged(() => Enums);
            }
        }

        IEnumerable<ITableInfoModel> IMsSqlCreator.Views
        {
            get { return Views; }
            set { }
        }

        private ThreadSaveObservableCollection<TableInfoViewModel> _views;

        IEnumerable<Dictionary<int, string>> IMsSqlCreator.Enums { get; }

        public ThreadSaveObservableCollection<TableInfoViewModel> Views
        {
            get { return _views; }
            set
            {
                SendPropertyChanging(() => Views);
                _views = value;
                SendPropertyChanged(() => Views);
            }
        }

        private ThreadSaveObservableCollection<IStoredPrcInfoModel> _storedProcs;

        public ThreadSaveObservableCollection<IStoredPrcInfoModel> StoredProcs
        {
            get { return _storedProcs; }
            set
            {
                SendPropertyChanging(() => StoredProcs);
                _storedProcs = value;
                SendPropertyChanged(() => StoredProcs);
            }
        }

        private string _connectionString;

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                SendPropertyChanging(() => ConnectionString);
                _connectionString = value;
                SendPropertyChanged(() => ConnectionString);
            }
        }

        private bool _connected;

        public bool NotConnected
        {
            get { return !Connected; }
        }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                SendPropertyChanging(() => NotConnected);
                SendPropertyChanging(() => Connected);
                _connected = value;
                SendPropertyChanged(() => NotConnected);
                SendPropertyChanged(() => Connected);
            }
        }

        public DelegateCommand ConnectToDatabaseCommand { get; private set; }

        private void ConnectToDatabaseExecute(object sender)
        {
            var dcd = new DataConnectionDialog();
            dcd.DataSources.Add(DataSource.SqlDataSource);

            if (DataConnectionDialog.Show(dcd) == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dcd.ConnectionString))
                    return;

                var sqlConnectionString = new SqlConnectionStringBuilder(dcd.ConnectionString);
                if (string.IsNullOrEmpty(sqlConnectionString.DataSource))
                {
                    this.Status = "Please provide a DataSource";
                    return;
                }

                this.ConnectionString = dcd.ConnectionString;
                base.SimpleWork(() =>
                {
                    this.CreateEntrys(ConnectionString, "C:\\", string.Empty);
                });
            }
        }

        private bool CanConnectToDatabaseExecute(object sender)
        {
            return !Connected && base.CheckCanExecuteCondition() && !IsEnumeratingDatabase;
        }

        private bool _isEnumeratingDatabase;

        public bool IsEnumeratingDatabase
        {
            get { return _isEnumeratingDatabase; }
            set
            {
                if (value == false)
                    Status = string.Format("Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action", Tables.Count, Views.Count, StoredProcs.Count);
                SendPropertyChanging(() => IsEnumeratingDatabase);
                _isEnumeratingDatabase = value;
                SendPropertyChanged(() => IsEnumeratingDatabase);
            }
        }

        public void CreateEntrys(string connection, string outputPath, string database)
        {
            Status = "Try to connect";
            //Data Source=(LocalDb)\ProjectsV12;Integrated Security=True;Database=TestDB;
            IsEnumeratingDatabase = true;
            TargetDir = outputPath;
            Manager = new DbAccessLayer(DbAccessType.MsSql, connection);
            DbConfig.EnableGlobalThreadSafety = true;
            try
            {
                Connected = Manager.CheckDatabase();
            }
            catch (Exception)
            {
                IsEnumeratingDatabase = false;
                Connected = false;
            }

            if (!Connected)
            {
                IsEnumeratingDatabase = false;
                Status = ("Database not accessible. Maybe wrong Connection or no Selected Database?");
                return;
            }
            var databaseName = string.IsNullOrEmpty(Manager.Database.DatabaseName) ? database : Manager.Database.DatabaseName;
            if (string.IsNullOrEmpty(databaseName))
            {
                IsEnumeratingDatabase = false;
                Status = ("Database not exists. Maybe wrong Connection or no Selected Database?");
                Connected = false;
                return;
            }
            Status = "Connection OK ... Reading Server Version ...";

            SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
            Status = "Reading Tables";

            int counter = 2;
            base.SimpleWorkWithSyncContinue(() =>
            {
                return
                    new DbAccessLayer(DbAccessType.MsSql, connection).Select<TableInformations>()
                        .Select(s => new TableInfoModel(s, databaseName, new DbAccessLayer(DbAccessType.MsSql, connection)))
                        .Select(s => new TableInfoViewModel(s, this))
                        .ToList();
            }, dbInfo =>
            {
                foreach (var source in dbInfo)
                {
                    if (Tables.All(f => f.Info.TableName != source.Info.TableName))
                        Tables.Add(source);
                }
                this.SelectedTable = Tables.FirstOrDefault();
                counter--;
                if (counter == 0)
                {
                    IsEnumeratingDatabase = false;
                    Status = "Done";
                }
            });
            base.SimpleWorkWithSyncContinue(() =>
            {
                return
                    new DbAccessLayer(DbAccessType.MsSql, connection)
                    .Select<ViewInformation>()
                    .Select(s => new TableInfoModel(s, databaseName, new DbAccessLayer(DbAccessType.MsSql, connection)))
                    .ToList();
            }, dbInfo =>
            {
                foreach (var source in dbInfo)
                {
                    if (Views.All(f => f.Info.TableName != source.Info.TableName))
                        Views.Add(source);
                }

                counter--;
                if (counter == 0)
                {
                    IsEnumeratingDatabase = false;
                    Status = "Done";
                }
            });
        }

        public void Compile()
        {
            var dir = new SaveFileDialog();
            DialogResult dirResult = DialogResult.Retry;
            base.ThreadSaveAction(() =>
            {
                dir.FileName = "dummy";
                dirResult = dir.ShowDialog();
            });
            if (dirResult == DialogResult.OK)
            {
                this.TargetDir = Path.GetDirectoryName(dir.FileName);
                foreach (var tableInfoModel in this.Tables)
                {
                    Status = String.Format("Compiling Table '{0}'", tableInfoModel.GetClassName());
                    SharedMethods.CompileTable(tableInfoModel, this);
                }
            }
        }

        IEnumerable<IStoredPrcInfoModel> IMsSqlCreator.StoredProcs
        {
            get { return this.StoredProcs; }
        }

        public string TargetDir { get; set; }
        private bool _generateConstructor;

        public bool GenerateConstructor
        {
            get { return _generateConstructor; }
            set
            {
                SendPropertyChanging(() => GenerateConstructor);
                _generateConstructor = value;
                SendPropertyChanged(() => GenerateConstructor);
            }
        }
        public DbAccessLayer Manager { get; set; }
        public string SqlVersion { get; set; }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                SendPropertyChanging(() => Status);
                _status = value;
                SendPropertyChanged(() => Status);
            }
        }

        private string _namespace;

        public string Namespace
        {
            get { return _namespace; }
            set
            {
                SendPropertyChanging(() => Namespace);
                _namespace = value;
                SendPropertyChanged(() => Namespace);
            }
        }

        private bool _generateForgeinKeyDeclarations;

        public bool GenerateForgeinKeyDeclarations
        {
            get { return _generateForgeinKeyDeclarations; }
            set
            {
                SendPropertyChanging(() => GenerateForgeinKeyDeclarations);
                _generateForgeinKeyDeclarations = value;
                SendPropertyChanged(() => GenerateForgeinKeyDeclarations);
            }
        }

        private bool _generateConfigMethod;

        public bool GenerateConfigMethod
        {
            get { return _generateConfigMethod; }
            set
            {
                SendPropertyChanging(() => GenerateConfigMethod);
                _generateConfigMethod = value;
                SendPropertyChanged(() => GenerateConfigMethod);
            }
        }

        private bool _generateCompilerHeader;

        public bool GenerateCompilerHeader
        {
            get { return _generateCompilerHeader; }
            set
            {
                SendPropertyChanging(() => GenerateCompilerHeader);
                _generateCompilerHeader = value;
                SendPropertyChanged(() => GenerateCompilerHeader);
            }
        }

        public DelegateCommand AddTableCommand { get; private set; }

        private void AddTableExecute(object sender)
        {
            this.Tables.Add(new TableInfoViewModel(new TableInfoModel()
            {
                Info = new TableInformations()
                {
                    TableName = "New Table"
                }
            }, this));
        }

        private bool CanAddTableExecute(object sender)
        {
            return true;
        }

        public DelegateCommand DeleteSelectedTableCommand { get; private set; }

        private void DeleteSelectedTableExecute(object sender)
        {
            this.Tables.Remove(this.SelectedTable);
        }

        private bool CanDeleteSelectedTableExecute(object sender)
        {
            return this.SelectedTable != null;
        }

        public DelegateCommand CompileCommand { get; private set; }

        private void CompileExecute(object sender)
        {
            base.SimpleWork(() =>
            {
                this.Compile();
            });
        }

        private bool CanCompileExecute(object sender)
        {
            return base.CheckCanExecuteCondition();
        }

        public DelegateCommand AdjustNamesCommand { get; private set; }

        private void AdjustNamesExecute(object sender)
        {
            base.SimpleWork(() =>
            {
                SharedMethods.AutoAlignNames(this.Tables);
            });
        }

        private bool CanAdjustNamesExecute(object sender)
        {
            return base.CheckCanExecuteCondition();
        }
    }

    public class DelegateLogger : ILogger
    {
        private readonly Action<string> _resolve;

        public DelegateLogger(Action<string> resolve)
        {
            _resolve = resolve;
        }

        public void Write(string content, params object[] arguments)
        {
            _resolve(string.Format(content, arguments));
        }

        public void WriteLine(string content = null, params object[] arguments)
        {
            if (content == null)
            {
                _resolve(Environment.NewLine);
                return;
            }
            _resolve(string.Format(content, arguments));
            _resolve(Environment.NewLine);
        }
    }
}
