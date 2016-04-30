using System;
using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;

namespace JPB.DataAccess.EntityCreator.Core.Models
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

		public IEnumerable<string> Compare(IColumInfoModel other)
		{
			if (this.NewColumnName != other.NewColumnName)
				yield return nameof(NewColumnName);
			if (this.IsRowVersion != other.IsRowVersion)
				yield return nameof(IsRowVersion);
			if (this.IsRowVersion != other.IsRowVersion)
				yield return nameof(IsRowVersion);
			if (this.PrimaryKey != other.PrimaryKey)
				yield return nameof(PrimaryKey);
			if (this.InsertIgnore != other.InsertIgnore)
				yield return nameof(InsertIgnore);
			if (this.EnumDeclaration != other.EnumDeclaration)
				yield return nameof(EnumDeclaration);
			if (this.Exclude != other.Exclude)
				yield return nameof(Exclude);
			if (this.ForgeinKeyDeclarations != other.ForgeinKeyDeclarations)
				yield return nameof(ForgeinKeyDeclarations);
		}

		public bool Equals(IColumInfoModel other)
		{
			return Equals(ColumnInfo, other.ColumnInfo) 
				&& string.Equals(NewColumnName, other.NewColumnName) 
				&& IsRowVersion == other.IsRowVersion 
				&& PrimaryKey == other.PrimaryKey 
				&& InsertIgnore == other.InsertIgnore 
				&& Equals(EnumDeclaration, other.EnumDeclaration) 
				&& Exclude == other.Exclude 
				&& Equals(ForgeinKeyDeclarations, other.ForgeinKeyDeclarations);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ColumInfoModel)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (ColumnInfo != null ? ColumnInfo.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (NewColumnName != null ? NewColumnName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ IsRowVersion.GetHashCode();
				hashCode = (hashCode * 397) ^ PrimaryKey.GetHashCode();
				hashCode = (hashCode * 397) ^ InsertIgnore.GetHashCode();
				hashCode = (hashCode * 397) ^ (EnumDeclaration != null ? EnumDeclaration.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Exclude.GetHashCode();
				hashCode = (hashCode * 397) ^ (ForgeinKeyDeclarations != null ? ForgeinKeyDeclarations.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}