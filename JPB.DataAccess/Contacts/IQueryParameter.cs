/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Data;

namespace JPB.DataAccess.Contacts
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
	}
}