namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface IColumInfoModel
	{
		ColumnInfo ColumnInfo { get; set; }
		string NewColumnName { get; set; }
		bool IsRowVersion { get; set; }
		bool PrimaryKey { get; set; }
		bool InsertIgnore { get; set; }
		EnumDeclarationModel EnumDeclaration { get; set; }
		bool Exclude { get; set; }
		ForgeinKeyInfoModel ForgeinKeyDeclarations { get; set; }
		string GetPropertyName();
	}
}