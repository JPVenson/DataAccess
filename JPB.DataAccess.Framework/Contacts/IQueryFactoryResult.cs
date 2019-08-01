#region

using System.Collections.Generic;

#endregion

namespace JPB.DataAccess.Framework.Contacts
{
	/// <summary>
	///     Marker interface for an QueryCommand that was created due the invoke of a Factory mehtod
	/// </summary>
	public interface IQueryFactoryResult
	{
		/// <summary>
		///     The SQL QueryCommand
		/// </summary>
		string Query { get; }

		/// <summary>
		///     Sql QueryCommand Parameter
		/// </summary>
		IEnumerable<IQueryParameter> Parameters { get; }
	}
}