using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Indicates this Property to be resolved by a ForeignKey column. A foreign key property must be public virtual the be detected
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ForeignKeyAttribute : InsertIgnoreAttribute
	{
		/// <summary>
		///		Defines a Foreign key relation where the <paramref name="foreignKey"/>
		/// is the key on this table and <paramref name="referenceKey"/> is the key on
		/// the other table
		/// </summary>
		public ForeignKeyAttribute(string foreignKey, string referenceKey)
		{
			ForeignKey = foreignKey;
			ReferenceKey = referenceKey;
		}

		/// <summary>
		///     The name of the Column that defines the ForginKey on this table
		/// </summary>
		public string ForeignKey { get; private set; }

		/// <summary>
		///		The name of the column that defines the targeted key on the reference table
		/// </summary>
		public string ReferenceKey { get; private set; }
	}
}