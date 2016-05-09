using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class TableMergeItem : AsyncViewModelBase
	{
		public TableMergeItem(ITableInfoModel left, ITableInfoModel right)
		{
			Left = left;
			Right = right;
			TableMerges = new ThreadSaveObservableCollection<PropertyMergeItem>();
			ColumnMergeItems = new ThreadSaveObservableCollection<ColumnMergeItem>();
		}

		public ITableInfoModel Left { get; private set; }

		public ITableInfoModel Right { get; private set; }

		private ThreadSaveObservableCollection<ColumnMergeItem> _columnMergeItems;

		public ThreadSaveObservableCollection<ColumnMergeItem> ColumnMergeItems
		{
			get { return _columnMergeItems; }
			set
			{
				SendPropertyChanging(() => ColumnMergeItems);
				_columnMergeItems = value;
				SendPropertyChanged(() => ColumnMergeItems);
			}
		}

		private ThreadSaveObservableCollection<PropertyMergeItem> _tableMerges;

		public ThreadSaveObservableCollection<PropertyMergeItem> TableMerges
		{
			get { return _tableMerges; }
			set
			{
				SendPropertyChanging(() => TableMerges);
				_tableMerges = value;
				SendPropertyChanged(() => TableMerges);
			}
		}
	}

	public class ColumnMergeItem : AsyncViewModelBase
	{
		public ColumnMergeItem(IColumInfoModel leftColumn, IColumInfoModel rightColumn, MergeStatus mergeStatus)
		{
			LeftColumn = leftColumn;
			RightColumn = rightColumn;
			MergeStatus = mergeStatus;
			ColumnMerges = new ThreadSaveObservableCollection<PropertyMergeItem>();
		}

		public IColumInfoModel LeftColumn { get; private set; }
		public IColumInfoModel RightColumn { get; private set; }

		public MergeStatus MergeStatus { get; private set; }

		private ThreadSaveObservableCollection<PropertyMergeItem> _columnMerges;

		public ThreadSaveObservableCollection<PropertyMergeItem> ColumnMerges
		{
			get { return _columnMerges; }
			set
			{
				SendPropertyChanging(() => ColumnMerges);
				_columnMerges = value;
				SendPropertyChanged(() => ColumnMerges);
			}
		}
	}
}