/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Collections.Generic;

namespace JPB.DataAccess.Contacts
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