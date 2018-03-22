using System;
using JPB.DataAccess.DbInfoConfig;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Indicates this Property to be resolved as a ForeignKey column
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ForeignKeyDeclarationAttribute : DataAccessAttribute
	{
		/// <summary>
		///     Declares a new Foreign key constraint
		/// </summary>
		/// <param name="foreignKey"></param>
		/// <param name="foreignTable"></param>
		public ForeignKeyDeclarationAttribute(string foreignKey, string foreignTable)
		{
			ForeignKey = foreignKey;
			ForeignTable = foreignTable;
		}

		/// <summary>
		///     Declares a new Foreign key constraint
		/// </summary>
		/// <param name="foreignKey"></param>
		/// <param name="foreignTable"></param>
		public ForeignKeyDeclarationAttribute(string foreignKey, Type foreignTable)
		{
			ForeignKey = foreignKey;
			ForeignType = foreignTable;
			ForeignTable = foreignTable.GetClassInfo().TableName;
		}

		/// <summary>
		///     Adds a new Foreign key based an a table by using its Primarykey
		/// </summary>
		public ForeignKeyDeclarationAttribute(Type forgeinTable)
		{
			ForeignType = forgeinTable;
			var classInfo = forgeinTable.GetClassInfo();
			ForeignTable = classInfo.TableName;
			if (classInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(
				string.Format("To use this constructor you have to define a Primary key on table {0} first", ForeignTable));
			}

			ForeignKey = classInfo.PrimaryKeyProperty.DbName;
		}

		/// <summary>
		///     The Key on the Foreign table
		/// </summary>
		public string ForeignKey { get; private set; }

		/// <summary>
		///     Table name of the Foreign constraint
		/// </summary>
		public string ForeignTable { get; private set; }

		/// <summary>
		///     The type of the table that is declared by ForginTable
		/// </summary>
		public Type ForeignType { get; private set; }
	}
}