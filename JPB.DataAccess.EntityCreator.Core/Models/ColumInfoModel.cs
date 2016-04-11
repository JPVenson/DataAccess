using System;
using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	[Serializable]
	public class ColumInfoModel : IColumInfoModel
	{
		public ColumInfoModel()
		{

		}

		public ColumInfoModel(ColumnInfo columnInfo)
		{
			ColumnInfo = columnInfo;
			if (columnInfo.TargetType2.ToLower() == "timestamp")
			{
				IsRowVersion = true;
			}
		}

		public IColumnInfo ColumnInfo { get; set; }
		public string NewColumnName { get; set; }
		public bool IsRowVersion { get; set; }
		public bool PrimaryKey { get; set; }
		public bool InsertIgnore { get; set; }
		public IEnumDeclarationModel EnumDeclaration { get; set; }
		public bool Exclude { get; set; }
		public IForgeinKeyInfoModel ForgeinKeyDeclarations { get; set; }

		public string GetPropertyName()
		{
			return string.IsNullOrEmpty(NewColumnName) ? ColumnInfo.ColumnName : NewColumnName;
		}
	}
}