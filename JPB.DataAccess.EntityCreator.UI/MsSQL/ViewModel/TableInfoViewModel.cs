using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class TableInfoViewModel : AsyncViewModelBase, ITableInfoModel
	{
		public TableInfoModel SourceElement { get; set; }

		public TableInfoViewModel(TableInfoModel sourceElement)
		{
			SourceElement = sourceElement;
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

		public List<ColumInfoModel> ColumnInfos
		{
			get { return SourceElement.ColumnInfos; }
			set
			{
				SourceElement.ColumnInfos = value;
				SendPropertyChanged();
			}
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
	}
}
