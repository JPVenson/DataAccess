using System;
using System.ComponentModel;
using System.Data;
using JPB.DataAccess.EntityCreator.Core;
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

			foreach (var sqlDefinedType in DbTypeToCsType.SqlDefinedTypes)
			{
				items.Add(sqlDefinedType.Value, string.Format("{0} - ({1})", sqlDefinedType.Key, sqlDefinedType.Value.Name));
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

		[Browsable(true)]
		[DisplayName("Type")]
		[ItemsSource(typeof(SqlDataTypeItemSource))]
		public Type TargetType
		{
			get { return ColumnInfo.TargetType; }
			set
			{
				ColumnInfo.TargetType = value;
				SendPropertyChanged();
			}
		}

		[Browsable(false)]
		public SqlDbType SqlType
		{
			get { return ColumnInfo.SqlType; }
			set
			{
				ColumnInfo.SqlType = value;
				SendPropertyChanged();
			}
		}

		//[DisplayName("Type")]
		//[ItemsSource(typeof(SqlDataTypeItemSource))]
		//public string TargetType2
		//{
		//	get { return ColumnInfo.TargetType2; }
		//	set
		//	{
		//		if(string.IsNullOrEmpty(value))
		//			return;

		//		ColumnInfo.TargetType2 = value;
		//		SendPropertyChanged();
		//	}
		//}
	}
}