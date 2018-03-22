using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Indicates this Property to be resolved by a ForeignKey column
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ForeignKeyAttribute : InsertIgnoreAttribute
	{
		/// <summary>
		///     The name of the Column that should be used
		/// </summary>
		public string KeyName { get; set; }
	}
}