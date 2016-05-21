using System;
using System.ComponentModel;
using System.Data;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.WPFBase.MVVM.ViewModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models
{
	public class SqlDataTypeItemSource : IItemsSource
	{
		public ItemCollection GetValues()
		{
			var items = new ItemCollection();

			foreach (SqlDbType sqlType in Enum.GetValues(typeof(SqlDbType)))
			{
				items.Add(sqlType.ToString());
			}

			return items;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class ColumnViewModel : AsyncViewModelBase, IColumnInfo
	{
		public ColumnViewModel(IColumnInfo columnInfo)
		{
			this.ColumnInfo = columnInfo;
		}

		[Browsable(false)]
		public new bool IsWorking { get; set; }

		[Browsable(false)]
		public new bool IsLocked { get; set; }

		[Browsable(false)]
		public new object Lock { get; set; }

		[Browsable(false)]
		public new bool IsNotWorking
		{
			get { return !IsWorking; }
		}

		[Browsable(false)]
		public IColumnInfo ColumnInfo { get; set; }

		[Description("The Column name")]
		[DisplayName("Column Name")]
		public string ColumnName
		{
			get { return ColumnInfo.ColumnName; }
			set
			{
				ColumnInfo.ColumnName = value;
				SendPropertyChanged();
			}
		}

		[Browsable(false)]
		public int PositionFromTop
		{
			get { return ColumnInfo.PositionFromTop; }
			set
			{
				ColumnInfo.PositionFromTop = value;
				SendPropertyChanged();
			}
		}

		public bool Nullable
		{
			get { return ColumnInfo.Nullable; }
			set
			{
				ColumnInfo.Nullable = value;
				SendPropertyChanged();
			}
		}

		[Browsable(false)]
		public Type TargetType
		{
			get { return ColumnInfo.TargetType; }
			set
			{
				ColumnInfo.TargetType = value;
				SendPropertyChanged();
			}
		}

		[DisplayName("Type")]
		[ItemsSource(typeof(SqlDataTypeItemSource))]
		public string TargetType2
		{
			get { return ColumnInfo.TargetType2; }
			set
			{
				if(string.IsNullOrEmpty(value))
					return;

				ColumnInfo.TargetType2 = value;
				SendPropertyChanged();
			}
		}
	}
}