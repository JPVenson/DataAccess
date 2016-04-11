using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface IColumInfoModel
	{
		IColumnInfo ColumnInfo { get; set; }
		string NewColumnName { get; set; }
		bool IsRowVersion { get; set; }
		bool PrimaryKey { get; set; }
		bool InsertIgnore { get; set; }
		IEnumDeclarationModel EnumDeclaration { get; set; }
		bool Exclude { get; set; }
		IForgeinKeyInfoModel ForgeinKeyDeclarations { get; set; }
		string GetPropertyName();
	}
}