/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Data;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Contacts
{
	public interface IDatabaseStrategy : ICloneable
	{
		/// <summary>
		///     Defines the database type this Strategy is used for
		/// </summary>
		DbAccessType SourceDatabase { get; }

		/// <summary>
		///     An Valid Connection string for the given Strategy
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		///     Optional used when connecting to a Local file
		/// </summary>
		string DatabaseFile { get; }

		/// <summary>
		///     Should return the current database if availibe
		/// </summary>
		string ServerName { get; }

		/// <summary>
		///     Creates a new Provider specific Connection that will held open until all actors want to close it
		/// </summary>
		/// <returns></returns>
		IDbConnection CreateConnection();

		IDbCommand CreateCommand(string strSql, IDbConnection conn);
		IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields);
		IDataParameter CreateParameter(string strName, object value);

		IDbDataAdapter CreateDataAdapter(IDbCommand cmd);

		DataTable CreateDataTable(string name, IDbCommand cmd);

		IDbCommand GetlastInsertedID_Cmd(IDbConnection conn);

		IDataPager<T> CreatePager<T>();

		IWrapperDataPager<T, TE> CreateConverterPager<T, TE>();

		/// <summary>
		///     Formarts a Command into a QueryCommand after the Strategy rules
		/// </summary>
		/// <returns></returns>
		string FormartCommandToQuery(IDbCommand command);


		/// <summary>
		///     Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <returns></returns>
		string ConvertParameter(DbType type);
	}
}