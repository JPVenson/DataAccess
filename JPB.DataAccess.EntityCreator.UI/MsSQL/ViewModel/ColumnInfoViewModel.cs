using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class ColumnInfoViewModel : AsyncViewModelBase, IColumInfoModel
	{
		public IColumInfoModel Model { get; set; }

		public ColumnInfoViewModel(IColumInfoModel model) : base(App.Current.Dispatcher)
		{
			Model = model;
		}

		public IColumnInfo ColumnInfo
		{
			get { return Model.ColumnInfo; }
			set
			{
				Model.ColumnInfo = value;
				SendPropertyChanged();
			}
		}

		public string NewColumnName
		{
			get { return Model.NewColumnName; }
			set
			{
				Model.NewColumnName = value;
				SendPropertyChanged();
			}
		}

		public bool IsRowVersion
		{
			get { return Model.IsRowVersion; }
			set
			{
				Model.IsRowVersion = value;
				SendPropertyChanged();
			}
		}

		public bool PrimaryKey
		{
			get { return Model.PrimaryKey; }
			set
			{
				Model.PrimaryKey = value;
				SendPropertyChanged();
			}
		}

		public bool InsertIgnore
		{
			get { return Model.InsertIgnore; }
			set
			{
				Model.InsertIgnore = value;
				SendPropertyChanged();
			}
		}

		public IEnumDeclarationModel EnumDeclaration
		{
			get { return Model.EnumDeclaration; }
			set
			{
				Model.EnumDeclaration = value;
				SendPropertyChanged();
			}
		}

		public bool Exclude
		{
			get { return Model.Exclude; }
			set
			{
				Model.Exclude = value;
				SendPropertyChanged();
			}
		}

		public IForgeinKeyInfoModel ForgeinKeyDeclarations
		{
			get { return Model.ForgeinKeyDeclarations; }
			set
			{
				Model.ForgeinKeyDeclarations = value;
				SendPropertyChanged();
			}
		}

		public string GetPropertyName()
		{
			return Model.GetPropertyName();
		}

		public IEnumerable<string> Compare(IColumInfoModel other)
		{
			return Model.Compare(other);
		}

		public bool Equals(IColumInfoModel other)
		{
			return Model.Equals(other);
		}
	}
}
