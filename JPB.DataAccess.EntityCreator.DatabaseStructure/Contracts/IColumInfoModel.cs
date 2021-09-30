using System;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IColumInfoModel : IElementComparer<IColumInfoModel>, IEquatable<IColumInfoModel>
	{
		IColumnInfo ColumnInfo { get; }
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