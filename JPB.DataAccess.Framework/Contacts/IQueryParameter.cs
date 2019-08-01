#region

using System;
using System.Data;

#endregion

namespace JPB.DataAccess.Framework.Contacts
{
	/// <summary>
	///     Wraps Paramters for Commands
	/// </summary>
	public interface IQueryParameter
	{
		/// <summary>
		///     The name of this Paramether with or without leeding @
		/// </summary>
		string Name { get; set; }

		/// <summary>
		///     The Real value that is given to Ado.net
		/// </summary>
		object Value { get; set; }

		/// <summary>
		///     In Certain cases ( as when using an NvarBinary column in MSSQL ) it is nessesary to declare the column type
		///     explicid
		/// </summary>
		Type SourceType { get; set; }

		/// <summary>
		///     The from SourceType resultung SourceDbType
		/// </summary>
		DbType SourceDbType { get; set; }

		/// <summary>
		///		Creates a Copy of the Current parameter
		/// </summary>
		/// <returns></returns>
		IQueryParameter Clone();
	}
}