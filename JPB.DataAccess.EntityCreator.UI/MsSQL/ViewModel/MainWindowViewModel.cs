using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Manager;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class MainWindowViewModel : AsyncViewModelBase, IEntryCreator, IMsSqlCreator
	{
		public MainWindowViewModel()
		{
			AdjustNamesCommand = new DelegateCommand(AdjustNamesExecute, CanAdjustNamesExecute);
			CompileCommand = new DelegateCommand(CompileExecute, CanCompileExecute);
			ConnectToDatabaseCommand = new DelegateCommand(ConnectToDatabaseExecute, CanConnectToDatabaseExecute);

			Tables = new ThreadSaveObservableCollection<TableInfoViewModel>();
			Views = new ThreadSaveObservableCollection<TableInfoModel>();
			StoredProcs = new ThreadSaveObservableCollection<StoredPrcInfoModel>();
			Enums = new ThreadSaveObservableCollection<Dictionary<int, string>>();
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

		private ThreadSaveObservableCollection<TableInfoModel> _views;

		IEnumerable<Dictionary<int, string>> IMsSqlCreator.Enums { get; }

		public ThreadSaveObservableCollection<TableInfoModel> Views
		{
			get { return _views; }
			set
			{
				SendPropertyChanging(() => Views);
				_views = value;
				SendPropertyChanged(() => Views);
			}
		}

		private ThreadSaveObservableCollection<StoredPrcInfoModel> _storedProcs;

		public ThreadSaveObservableCollection<StoredPrcInfoModel> StoredProcs
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

		public bool Connected
		{
			get { return _connected; }
			set
			{
				SendPropertyChanging(() => Connected);
				_connected = value;
				SendPropertyChanged(() => Connected);
			}
		}

		public DelegateCommand ConnectToDatabaseCommand { get; private set; }

		private void ConnectToDatabaseExecute(object sender)
		{
			base.SimpleWork(() =>
			{
				this.CreateEntrys(ConnectionString, TargetDir, string.Empty);
			});
		}

		private bool CanConnectToDatabaseExecute(object sender)
		{
			return !Connected && !string.IsNullOrEmpty(ConnectionString) && base.CheckCanExecuteCondition();
		}

		public void CreateEntrys(string connection, string outputPath, string database)
		{
			TargetDir = outputPath;
			Manager = new DbAccessLayer(DbAccessType.MsSql, connection);
			try
			{
				Connected = Manager.CheckDatabase();
			}
			catch (Exception)
			{
				Connected = false;
			}

			if (!Connected)
			{
				Status = ("Database not accessible. Maybe wrong Connection or no Selected Database?");
				return;
			}
			var databaseName = string.IsNullOrEmpty(Manager.Database.DatabaseName) ? database : Manager.Database.DatabaseName;
			if (string.IsNullOrEmpty(databaseName))
			{
				Status = ("Database not exists. Maybe wrong Connection or no Selected Database?");
				return;
			}
			Status = "Connection OK ... Reading Server Version ...";

			SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();
			foreach (var source in Manager.Select<TableInformations>().Select(s => new TableInfoModel(s, databaseName)).Select(s => new TableInfoViewModel(s)).ToList())
			{
				Tables.Add(source);
			}

			foreach (var source in Manager.Select<ViewInformation>().Select(s => new TableInfoModel(s, databaseName)).ToList())
			{
				Views.Add(source);
			}

			foreach (var source in Manager.Select<StoredProcedureInformation>().Select(s => new StoredPrcInfoModel(s)).ToList())
			{
				StoredProcs.Add(source);
			}

			Status = string.Format("Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action", Tables.Count, Views.Count, StoredProcs.Count);
		}

		public void Compile()
		{
			foreach (var tableInfoModel in this.Tables)
			{
				SharedMethods.CompileTable(tableInfoModel, this);
			}
		}

		IEnumerable<StoredPrcInfoModel> IMsSqlCreator.StoredProcs {
		get { return this.StoredProcs; } }
		public string TargetDir { get; set; }
		public bool GenerateConstructor { get; set; }
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

		private bool _generateAdoConstructor;

		public bool GenerateAdoConstructor
		{
			get { return _generateAdoConstructor; }
			set
			{
				SendPropertyChanging(() => GenerateAdoConstructor);
				_generateAdoConstructor = value;
				SendPropertyChanged(() => GenerateAdoConstructor);
			}
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
}
