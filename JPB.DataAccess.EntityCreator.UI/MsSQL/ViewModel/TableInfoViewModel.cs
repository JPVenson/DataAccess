﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.EntityCreator.UI.MsSQL.View;
using JPB.ErrorValidation;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class TableInfoViewModel : DataErrorBase<TableInfoViewModel, TableInfoModelErrorProvider>, ITableInfoModel
	{
		private readonly SqlEntityCreatorViewModel _compilerOptions;
		public ITableInfoModel SourceElement { get; set; }

		public TableInfoViewModel(ITableInfoModel sourceElement, SqlEntityCreatorViewModel compilerOptions) : base(App.Current.Dispatcher)
		{
			_compilerOptions = compilerOptions;
			SourceElement = sourceElement;
			ColumnInfoModels = new ThreadSaveObservableCollection<ColumnInfoViewModel>(SourceElement.ColumnInfos.Select(f => new ColumnInfoViewModel(f)));
			CreatePreviewCommand = new DelegateCommand(CreatePreviewExecute, CanCreatePreviewExecute);
		}

		public TableInformations Info
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

		private IColumInfoModel _selectedColumn;

		public IColumInfoModel SelectedColumn
		{
			get { return _selectedColumn; }
			set
			{
				SendPropertyChanging(() => SelectedColumn);
				_selectedColumn = value;
				SendPropertyChanged(() => SelectedColumn);
			}
		}

		private ThreadSaveObservableCollection<ColumnInfoViewModel> _columnInfoModels;

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

		public string GetClassName()
		{
			return ((ITableInfoModel) SourceElement).GetClassName();
		}

		public DelegateCommand CreatePreviewCommand { get; private set; }

		private void CreatePreviewExecute(object sender)
		{
			var previewWindwo = new ClassPreviewWindow();
			previewWindwo.DataContext = new ClassPreviewViewModel(this, _compilerOptions);
			previewWindwo.Show();
		}

		private bool CanCreatePreviewExecute(object sender)
		{
			return this._compilerOptions.SelectedTable != null;
		}
	}
}
