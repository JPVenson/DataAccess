using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.Shared
{
	public class SearchViewModel : AsyncViewModelBase
	{
		public SearchViewModel()
		{
			
		}

		private DbClassInfoCache _targetType;

		public DbClassInfoCache TargetType
		{
			get { return _targetType; }
			set
			{
				SendPropertyChanging(() => TargetType);
				_targetType = value;
				SendPropertyChanged(() => TargetType);
			}
		}

		private ObservableCollection<SearchQueryPart> _searchQueryParts;

		public ObservableCollection<SearchQueryPart> SearchQueryParts
		{
			get { return _searchQueryParts; }
			set
			{
				SendPropertyChanging(() => SearchQueryParts);
				_searchQueryParts = value;
				SendPropertyChanged(() => SearchQueryParts);
			}
		}
	}

	public class SearchQueryPart : AsyncViewModelBase
	{
		private SearchColumnInfo _searchColumnInfo;
		private SearchColumnOperator _operator;

		public SearchColumnOperator Operator
		{
			get { return _operator; }
			set
			{
				SendPropertyChanging(() => Operator);
				_operator = value;
				SendPropertyChanged(() => Operator);
			}
		}

		public SearchColumnInfo SearchColumnInfo
		{
			get { return _searchColumnInfo; }
			set
			{
				SendPropertyChanging(() => SearchColumnInfo);
				_searchColumnInfo = value;
				SendPropertyChanged(() => SearchColumnInfo);
			}
		}
	}

	public class SearchColumnOperator
	{
		public ISearchOperator Operator { get; set; }
	}

	public interface ISearchOperator
	{
		string OperatorDisplay { get; }
	}

	public class SearchColumnInfo
	{
		public string ColumnName { get; set; }
	}
}
