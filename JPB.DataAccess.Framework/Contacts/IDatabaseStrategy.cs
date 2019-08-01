#region

using System;
using System.Data;
using JPB.DataAccess.Framework.Contacts.Pager;
using JPB.DataAccess.Framework.Manager;

#endregion

namespace JPB.DataAccess.Framework.Contacts
{
	/// <summary>
	///     A Strategy for accessing a Database Provider
	/// </summary>
	/// <seealso cref="System.ICloneable" />
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

		/// <summary>
		///     Creates a command.
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="conn">The connection.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		IDbCommand CreateCommand(string strSql, IDbConnection conn, params IDataParameter[] fields);

		/// <summary>
		///     Creates a query parameter.
		/// </summary>
		/// <param name="strName">Name of the string.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		IDataParameter CreateParameter(string strName, object value);

		/// <summary>
		///     Getlasts a inserted identifier command.
		/// </summary>
		/// <param name="conn">The connection.</param>
		/// <returns></returns>
		IDbCommand GetlastInsertedID_Cmd(IDbConnection conn);

		/// <summary>
		///     Creates a data pager.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDataPager<T> CreatePager<T>();

		/// <summary>
		///     Creates the a pager that can convert each item.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE">The type of the e.</typeparam>
		/// <returns></returns>
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

		/// <summary>
		///     Calls the API to close all open Connections and free the Database
		/// </summary>
		void CloseAllConnections();

		///  <summary>
		/// 		Enables the Identity Insert/Update on a single table
		///  </summary>
		///  <param name="classInfoTableName"></param>
		/// <param name="conn"></param>
		IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn);

		///  <summary>
		/// 		Disables the Identity Insert/Update on a single table
		///  </summary>
		///  <param name="classInfoTableName"></param>
		/// <param name="conn"></param>
		IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn);
	}
}