#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.Core.Poco.MsSQL;
using JPB.DataAccess.EntityCreator.UI.MsSQL.Services;
using JPB.DataAccess.EntityCreator.UI.MsSQL.View;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models;
using JPB.ErrorValidation.ViewModelProvider;
using JPB.ErrorValidation.ViewModelProvider.Base;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

#endregion

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	[DebuggerDisplay("{" + nameof(GetClassName) + "()}")]
	public class TableInfoViewModel : AsyncErrorProviderBase<TableInfoModelErrorProvider>, ITableInfoModel
	{
		private readonly SqlEntityCreatorViewModel _compilerOptions;

		private ThreadSaveObservableCollection<ColumnInfoViewModel> _columnInfoModels;

		private ColumnInfoViewModel _selectedColumn;

		public TableInfoViewModel(ITableInfoModel sourceElement, SqlEntityCreatorViewModel compilerOptions)
		{
			_compilerOptions = compilerOptions;
			SourceElement = sourceElement;
			CreatePreviewCommand = new DelegateCommand(CreatePreviewExecute, CanCreatePreviewExecute);
			AddColumnCommand = new DelegateCommand(AddColumnExecute, CanAddColumnExecute);
			RemoveColumnCommand = new DelegateCommand(RemoveColumnExecute, CanRemoveColumnExecute);
			ColumnInfoModels = new ThreadSaveObservableCollection<ColumnInfoViewModel>();
			Refresh();
		}

		public ITableInfoModel SourceElement { get; set; }

		[ExpandableObject]
		public ITableInformations Info
		{
			get { return SourceElement.Info; }
			set
			{
				SourceElement.Info = value;
				SendPropertyChanged();
			}
		}

		public string Database
		{
			get { return SourceElement.Database; }
			set
			{
				SourceElement.Database = value;
				SendPropertyChanged();
			}
		}

		public ColumnInfoViewModel SelectedColumn
		{
			get { return _selectedColumn; }
			set
			{
				SendPropertyChanging(() => SelectedColumn);
				_selectedColumn = value;
				SendPropertyChanged(() => SelectedColumn);
			}
		}

		public ThreadSaveObservableCollection<ColumnInfoViewModel> ColumnInfoModels
		{
			get { return _columnInfoModels; }
			set
			{
				SendPropertyChanging(() => ColumnInfoModels);
				_columnInfoModels = value;
				SendPropertyChanged(() => ColumnInfoModels);
			}
		}

		public IEnumerable<IColumInfoModel> ColumnInfos
		{
			get { return ColumnInfoModels; }
		}

		public string NewTableName
		{
			get { return SourceElement.NewTableName; }
			set
			{
				SourceElement.NewTableName = value;
				SendPropertyChanged();
			}
		}

		public bool Exclude
		{
			get { return SourceElement.Exclude; }
			set
			{
				SourceElement.Exclude = value;
				SendPropertyChanged();
			}
		}

		public bool CreateFallbackProperty
		{
			get { return SourceElement.CreateFallbackProperty; }
			set
			{
				SourceElement.CreateFallbackProperty = value;
				SendPropertyChanged();
			}
		}

		public bool CreateSelectFactory
		{
			get { return SourceElement.CreateSelectFactory; }
			set
			{
				SourceElement.CreateSelectFactory = value;
				SendPropertyChanged();
			}
		}

		public bool CreateDataRecordLoader
		{
			get { return SourceElement.CreateDataRecordLoader; }
			set
			{
				SourceElement.CreateDataRecordLoader = value;
				SendPropertyChanged();
			}
		}

		public DelegateCommand RemoveColumnCommand { get; private set; }

		public DelegateCommand AddColumnCommand { get; private set; }

		public DelegateCommand CreatePreviewCommand { get; private set; }

		public string GetClassName()
		{
			return SourceElement.GetClassName();
		}

		private void RemoveColumnExecute(object sender)
		{
			ColumnInfoModels.Remove(SelectedColumn);
		}

		private bool CanRemoveColumnExecute(object sender)
		{
			return SelectedColumn != null;
		}

		private void AddColumnExecute(object sender)
		{
			var columnData = new ColumnInfo
			{
				ColumnName = "New Column",
				TargetType = typeof(object)
			};
			AddColumn(columnData);
		}

		public void AddColumn(IColumnInfo column)
		{
			var columnMeta = new ColumnInfoModel(column);
			var collumnVm = new ColumnInfoViewModel(columnMeta);
			ColumnInfoModels.Add(collumnVm);
		}

		private bool CanAddColumnExecute(object sender)
		{
			return true;
		}

		private void CreatePreviewExecute(object sender)
		{
			var previewWindwo = new ClassPreviewWindow();
			previewWindwo.DataContext = new ClassPreviewViewModel(this, _compilerOptions);
			previewWindwo.Show();
		}

		private bool CanCreatePreviewExecute(object sender)
		{
			return _compilerOptions.SelectedTable != null;
		}

		public void Refresh()
		{
			ColumnInfoModels.Clear();
			foreach (var result in SourceElement.ColumnInfos.Select(f => new ColumnInfoViewModel(f)))
			{
				ColumnInfoModels.Add(result);
			}

			SendPropertyChanged(string.Empty);
		}
	}
}