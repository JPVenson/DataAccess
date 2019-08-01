using System;
using JPB.DataAccess.Framework.DbInfoConfig;

namespace JPB.DataAccess.Framework.ModelsAnotations
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
		public ForeignKeyDeclarationAttribute(string foreignKey, Type foreignTable)
		{
			ForeignKey = foreignKey;
			ForeignType = foreignTable;
		}

		/// <summary>
		///     Adds a new Foreign key based an a table by using its Primarykey
		/// </summary>
		public ForeignKeyDeclarationAttribute(Type forgeinTable)
		{
			ForeignType = forgeinTable;
		}

		/// <summary>
		///		Gets the compiled result of <see cref="ForeignKeyDeclarationAttribute"/>
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public ForeignDeclarationInfo CompileInfoWith(DbConfig config)
		{
			var classInfo = ForeignType != null ? config.GetOrCreateClassInfoCache(ForeignType) : null;
			return new ForeignDeclarationInfo(ForeignKey ?? classInfo.PrimaryKeyProperty.DbName,
				ForeignTable ?? classInfo.TableName, ForeignType);
		}

		/// <summary>
		///		Returns the compiled result for the info inside ForeignKeyDeclarationAttribute
		/// </summary>
		public struct ForeignDeclarationInfo
		{
			internal ForeignDeclarationInfo(string foreignKey, string foreignTable, Type foreignType)
			{
				ForeignKey = foreignKey;
				ForeignTable = foreignTable;
				ForeignType = foreignType;
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

		/// <summary>
		///     The Key on the Foreign table
		/// </summary>
		private string ForeignKey { get; }

		/// <summary>
		///     Table name of the Foreign constraint
		/// </summary>
		private string ForeignTable { get; }

		/// <summary>
		///     The type of the table that is declared by ForginTable
		/// </summary>
		private Type ForeignType { get; }
	}
}