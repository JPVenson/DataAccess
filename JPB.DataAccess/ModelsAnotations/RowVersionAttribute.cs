using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     A rowstate that is used to Detect a newer version
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RowVersionAttribute : InsertIgnoreAttribute
	{
	}
}