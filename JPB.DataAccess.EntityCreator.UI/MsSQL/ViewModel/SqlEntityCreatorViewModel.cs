using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AutoMapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.EntityCreator.Core;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models;
using JPB.DataAccess.Manager;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Data.ConnectionUI;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class SqlEntityCreatorViewModel : AsyncViewModelBase, IMsSqlCreator
	{
		private bool _connected;

		private string _connectionString;

		private ThreadSaveObservableCollection<Dictionary<int, string>> _enums;

		private bool _generateCompilerHeader;

		private bool _generateConfigMethod;
		private bool _generateConstructor;

		private bool _generateForgeinKeyDeclarations;

		private bool _isEnumeratingDatabase;

		private string _namespace;

		private TableInfoViewModel _selectedTable;

		private string _status;

		private ThreadSaveObservableCollection<IStoredPrcInfoModel> _storedProcs;

		private ThreadSaveObservableCollection<TableInfoViewModel> _tables;

		private ThreadSaveObservableCollection<TableInfoViewModel> _views;

		public SqlEntityCreatorViewModel()
		{
			AdjustNamesCommand = new DelegateCommand(AdjustNamesExecute, CanAdjustNamesExecute);
			CompileCommand = new DelegateCommand(CompileExecute, CanCompileExecute);
			ConnectToDatabaseCommand = new DelegateCommand(ConnectToDatabaseExecute, CanConnectToDatabaseExecute);
			SaveConfigCommand = new DelegateCommand(SaveConfigExecute, CanSaveConfigExecute);
			LoadConfigCommand = new DelegateCommand(LoadConfigExecute, CanLoadConfigExecute);
			OpenInfoWindowCommand = new DelegateCommand(OpenInfoWindowExecute);
			DeleteSelectedTableCommand = new DelegateCommand(DeleteSelectedTableExecute, CanDeleteSelectedTableExecute);
			AddTableCommand = new DelegateCommand(AddTableExecute, CanAddTableExecute);

			Tables = new ThreadSaveObservableCollection<TableInfoViewModel>();
			Views = new ThreadSaveObservableCollection<TableInfoViewModel>();
			StoredProcs = new ThreadSaveObservableCollection<IStoredPrcInfoModel>();
			Enums = new ThreadSaveObservableCollection<Dictionary<int, string>>();

			SharedMethods.Logger = new DelegateLogger(message => Status = message);
		}

		public DelegateCommand OpenInfoWindowCommand { get; set; }

		public DelegateCommand LoadConfigCommand { get; private set; }

		public DelegateCommand SaveConfigCommand { get; private set; }

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

		public bool IsEnumeratingDatabase
		{
			get { return _isEnumeratingDatabase; }
			set
			{
				if (value == false)
					Status =
						string.Format("Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action",
							Tables.Count, Views.Count, StoredProcs.Count);
				SendPropertyChanging(() => IsEnumeratingDatabase);
				_isEnumeratingDatabase = value;
				SendPropertyChanged(() => IsEnumeratingDatabase);
			}
		}

		public DbAccessLayer Manager { get; set; }

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

		public DelegateCommand AddTableCommand { get; private set; }

		public DelegateCommand DeleteSelectedTableCommand { get; private set; }

		public DelegateCommand CompileCommand { get; private set; }

		public DelegateCommand AdjustNamesCommand { get; private set; }

		IEnumerable<ITableInfoModel> IMsSqlCreator.Tables
		{
			get { return Tables; }
			set { }
		}

		IEnumerable<ITableInfoModel> IMsSqlCreator.Views
		{
			get { return Views; }
			set { }
		}

		IEnumerable<Dictionary<int, string>> IMsSqlCreator.Enums { get; }

		public void CreateEntrys(string connection, string outputPath, string database)
		{
			CreateEntrysAsync(connection, outputPath, database);
		}

		public async Task CreateEntrysAsync(string connection, string outputPath, string database)
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
				Status = "Database not accessible. Maybe wrong Connection or no Selected Database?";
				return;
			}
			var databaseName = string.IsNullOrEmpty(Manager.Database.DatabaseName) ? database : Manager.Database.DatabaseName;
			if (string.IsNullOrEmpty(databaseName))
			{
				IsEnumeratingDatabase = false;
				Status = "Database not exists. Maybe wrong Connection or no Selected Database?";
				Connected = false;
				return;
			}
			Status = "Connection OK ... Reading Server Version ...";

			SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
			Status = "Reading Tables";

			var counter = 2;
			var createTables = SimpleWorkWithSyncContinue(() =>
			{
				return
					new DbAccessLayer(DbAccessType.MsSql, connection).Select<TableInformations>()
						.Select(s => new TableInfoModel(s, databaseName, new DbAccessLayer(DbAccessType.MsSql, connection)))
						.Select(s => new TableInfoViewModel(s, this))
						.ToList();
			}, dbInfo =>
			{
				foreach (var source in dbInfo)
					if (Tables.All(f => f.Info.TableName != source.Info.TableName))
						Tables.Add(source);
			});
			var createViews = SimpleWorkWithSyncContinue(() =>
			{
				return
					new DbAccessLayer(DbAccessType.MsSql, connection)
						.Select<ViewInformation>()
						.Select(s => new TableInfoModel(s, databaseName, new DbAccessLayer(DbAccessType.MsSql, connection)))
						.ToList();
			}, dbInfo =>
			{
				foreach (var source in dbInfo)
					if (Views.All(f => f.Info.TableName != source.Info.TableName))
						Views.Add(source);
			});

			await createTables;
			await createViews;
			SelectedTable = Tables.FirstOrDefault();

			IsEnumeratingDatabase = false;
			Status = "Done";
		}

		public void Compile()
		{
			var dir = new SaveFileDialog();
			var dirResult = DialogResult.Retry;
			ThreadSaveAction(() =>
			{
				dir.FileName = "dummy";
				dirResult = dir.ShowDialog();
			});
			if (dirResult == DialogResult.OK)
			{
				TargetDir = Path.GetDirectoryName(dir.FileName);
				foreach (var tableInfoModel in Tables)
				{
					Status = string.Format("Compiling Table '{0}'", tableInfoModel.GetClassName());
					SharedMethods.CompileTable(tableInfoModel, this);
				}
			}
		}

		IEnumerable<IStoredPrcInfoModel> IMsSqlCreator.StoredProcs
		{
			get { return StoredProcs; }
		}

		public string TargetDir { get; set; }

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

		public string SqlVersion { get; set; }

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

		public void OpenInfoWindowExecute(object sender)
		{
			new InfoWindow().ShowDialog();
		}

		private void LoadConfigExecute(object sender)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = false;
			fileDialog.CheckFileExists = true;
			fileDialog.DefaultExt = "*.msConfigStore";
			fileDialog.Filter = "ConfigFile (*.msConfigStore)|*.msConfigStore";
			var result = fileDialog.ShowDialog();
			if (result.HasValue && result.Value && File.Exists(fileDialog.FileName))
			{
				Tables.Clear();
				Views.Clear();
				StoredProcs.Clear();

				var binFormatter = new BinaryFormatter();
				ConfigStore options;
				try
				{
					using (var fs = fileDialog.OpenFile())
					{
						options = (ConfigStore) binFormatter.Deserialize(fs);
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
					var messageBoxResult = MessageBox.Show(Application.Current.MainWindow,
						"Warning Version missmatch",
						string.Format("The current Entity Creator version ({0}) is not equals the version ({1}) you have provided.",
							version, options.Version),
						MessageBoxButton.OKCancel);

					if (messageBoxResult == MessageBoxResult.Cancel)
						return;
				}

				if (options.SourceConnectionString != null)
				{
					ConnectionString = options.SourceConnectionString;
					CreateEntrysAsync(ConnectionString, "", string.Empty).ContinueWith((task) =>
					{
						var mapper = new AutoMapper.Mapper(new MapperConfiguration((config) =>
						{
							config.CreateMissingTypeMaps = true;

							config.CreateMap<TableInfoModel, TableInfoModel>();
							config.CreateMap<ColumInfoModel, ColumInfoModel>();
							config.CreateMap<ColumnInfo, ColumnInfo>();
							config.CreateMap<EnumDeclarationModel, EnumDeclarationModel>();
							config.CreateMap<ForgeinKeyInfoModel, ForgeinKeyInfoModel>();
							config.CreateMap<TableInformations, TableInformations>();

							config.CreateMap<StoredPrcInfoModel, StoredPrcInfoModel>();
						}));

						var defaultContextMapper = mapper.DefaultContext.Mapper;

						foreach (var option in options.Tables)
						{
							var itemExisits = Tables.FirstOrDefault(s => s.Info.TableName == option.Info.TableName);
							if (itemExisits != null)
							{
								defaultContextMapper.Map(option, itemExisits.SourceElement);
								itemExisits.Refresh();
							}
							else
							{
								Tables.Add(new TableInfoViewModel(option, this));
							}
						}
						foreach (var option in options.Views)
						{
							var hasView = Views.FirstOrDefault(f => f.Info.TableName == option.Info.TableName);

							if (hasView != null)
							{
								defaultContextMapper.Map(option, hasView.SourceElement);
								hasView.Refresh();
							}
							else
							{
								Views.Add(new TableInfoViewModel(option, this));
							}
						}
						SelectedTable = Tables.FirstOrDefault();
					});
				}

				//foreach (var option in options.StoredPrcInfoModels)
				//{
				//	StoredProcs.Add(option);
				//}
				GenerateConstructor = options.GenerateConstructor;
				GenerateForgeinKeyDeclarations = options.GenerateForgeinKeyDeclarations;
				GenerateCompilerHeader = options.GenerateCompilerHeader;
				GenerateConfigMethod = options.GenerateConfigMethod;
				Namespace = options.Namespace;
				TargetDir = options.TargetDir;
			}
		}

		private bool CanLoadConfigExecute(object sender)
		{
			return !Connected;
		}

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
				options.StoredPrcInfoModels = StoredProcs.ToList();
				options.Views = Views.Select(s => s.SourceElement).ToList();
				options.Tables = Tables.Select(s => s.SourceElement).ToList();

				options.GenerateConstructor = GenerateConstructor;
				options.GenerateForgeinKeyDeclarations = GenerateForgeinKeyDeclarations;
				options.GenerateCompilerHeader = GenerateCompilerHeader;
				options.GenerateConfigMethod = GenerateConfigMethod;
				options.Namespace = Namespace;
				options.TargetDir = TargetDir;

				var version = typeof(SharedMethods).Assembly.GetName().Version;

				options.Version = version.ToString();

				if (ConnectionString != null)
					options.SourceConnectionString = ConnectionString;

				if (File.Exists(fileDialog.FileName))
					File.Delete(fileDialog.FileName);
				using (var fs = fileDialog.OpenFile())
				{
					new BinaryFormatter().Serialize(fs, options);
				}
			}
		}

		private bool CanSaveConfigExecute(object sender)
		{
			return StoredProcs.Any() || Views.Any() || Tables.Any();
		}

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
					Status = "Please provide a DataSource";
					return;
				}

				ConnectionString = dcd.ConnectionString;
				CreateEntrysAsync(ConnectionString, "C:\\", string.Empty);
			}
		}

		private bool CanConnectToDatabaseExecute(object sender)
		{
			return !Connected && CheckCanExecuteCondition() && !IsEnumeratingDatabase;
		}

		private void AddTableExecute(object sender)
		{
			Tables.Add(new TableInfoViewModel(new TableInfoModel
			{
				Info = new TableInformations
				{
					TableName = "New Table"
				}
			}, this));
		}

		private bool CanAddTableExecute(object sender)
		{
			return true;
		}

		private void DeleteSelectedTableExecute(object sender)
		{
			Tables.Remove(SelectedTable);
		}

		private bool CanDeleteSelectedTableExecute(object sender)
		{
			return SelectedTable != null;
		}

		private void CompileExecute(object sender)
		{
			SimpleWork(() => { Compile(); });
		}

		private bool CanCompileExecute(object sender)
		{
			return CheckCanExecuteCondition();
		}

		private void AdjustNamesExecute(object sender)
		{
			SimpleWork(() => { SharedMethods.AutoAlignNames(Tables); });
		}

		private bool CanAdjustNamesExecute(object sender)
		{
			return CheckCanExecuteCondition();
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