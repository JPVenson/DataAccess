#region

using System;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Database wrapper interface
	/// </summary>
	public interface IDatabase : IDisposable
	{
		/// <summary>
		///     Should additional Query infos be saved?
		/// </summary>
		bool Debugger { get; set; }

		/// <summary>
		/// Set the NestedTransaction option.
		/// If set to true when calling an BeginTransaction will succeed when an open connection was done and will be "Attached" to that connection.
		/// Otherwise it will throw an exception
		/// </summary>
		bool AllowNestedTransactions { get; set; }

		/// <summary>
		///     Defines the Target database we are conneting to
		/// </summary>
		DbAccessType TargetDatabase { get; }

		/// <summary>
		///     NotImp
		/// </summary>
		bool IsAttached { get; }

		/// <summary>
		///     Get the Current Connection string
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		///     If local instance get the file
		/// </summary>
		string DatabaseFile { get; }

		/// <summary>
		///     Get the Database name that we are connected to
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		///     Get the Server we are Connected to
		/// </summary>
		string ServerName { get; }

		/// <summary>
		///     Get the last Executed QueryCommand wrapped by a Debugger
		/// </summary>
		QueryDebugger LastExecutedQuery { get; }

		/// <summary>
		///     Should always be called just before executing the given Command
		/// </summary>
		/// <param name="cmd"></param>
		void PrepaireRemoteExecution(IDbCommand cmd);

		/// <summary>
		///     Get Database specific Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDataPager<T> CreatePager<T>();

		/// <summary>
		///     Get database specific converter Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		IWrapperDataPager<T, TE> CreatePager<T, TE>();

		/// <summary>
		///     Required
		///     Is used to attach a Strategy that handles certain kinds of Databases
		/// </summary>
		void Attach(IDatabaseStrategy strategy);

		/// <summary>
		///     Required
		///     Is used to create an new Connection based on the Strategy and
		///     keep it
		/// </summary>
		/// <returns></returns>
		IDbConnection GetConnection();

		/// <summary>
		///     Required
		///     When a new Connection is requested this function is used
		/// </summary>
		void Connect(IsolationLevel? levl = null);

		/// <summary>
		///     rollback the current Transaction.
		/// </summary>
		void TransactionRollback();

		/// <summary>
		///     Required
		///     Closing a open Connection
		/// </summary>
		void CloseConnection();

		/// <summary>
		///     Required
		///     Closing all Connections that maybe open
		/// </summary>
		void CloseAllConnection();

		/// <summary>
		///     Executes a Query that returns no data
		/// </summary>
		/// <param name="strSql">The SQL.</param>
		/// <param name="obj">Arguments</param>
		/// <returns></returns>
		int ExecuteNonQuery(string strSql, params object[] obj);

		/// <summary>
		///     Executes a Query that returns no data
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		int ExecuteNonQuery(IDbCommand cmd);

		/// <summary>
		///     Gets the data reader for the given Sql Statement.
		/// </summary>
		/// <param name="strSql">The SQL.</param>
		/// <param name="obj">Arguments.</param>
		/// <returns></returns>
		IDataReader GetDataReader(string strSql, params object[] obj);

		/// <summary>
		///     Gets a single Value from the Query
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <returns></returns>
		object GetSkalar(IDbCommand cmd);

		/// <summary>
		///     Gets a single Value from the Query
		/// </summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		object GetSkalar(string strSql, params object[] obj);

		//DataTable GetDataTable(string name, string strSql);
		//DataSet GetDataSet(string strSql);

		/// <summary>
		///     Required
		///     Creates a Command based on the Strategy
		/// </summary>
		/// <returns></returns>
		IDbCommand CreateCommand(string strSql, params IDataParameter[] fields);

		/// <summary>
		///     Required
		///     Creates a Parameter based on the Strategy
		/// </summary>
		/// <returns></returns>
		IDataParameter CreateParameter(string strName, object value);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void Run(Action<IDatabase> action);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		T Run<T>(Func<IDatabase, T> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		Task RunAsync(Func<IDatabase, Task> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void RunInTransaction(Action<IDatabase> action);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void RunInTransaction(Action<IDatabase> action, IsolationLevel transaction);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		Task<T> RunInTransactionAsync<T>(Func<IDatabase, Task<T>> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		Task<T> RunInTransactionAsync<T>(Func<IDatabase, Task<T>> func, IsolationLevel transaction);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		T RunInTransaction<T>(Func<IDatabase, T> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		T RunInTransaction<T>(Func<IDatabase, T> func, IsolationLevel transaction);

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <returns></returns>
		IDatabase Clone();

		/// <summary>
		///     Getlasts the inserted identifier command.
		/// </summary>
		/// <returns></returns>
		IDbCommand GetlastInsertedIdCommand();

		/// <summary>
		///     Formarts a Command to a executable QueryCommand
		/// </summary>
		/// <returns></returns>
		string FormartCommandToQuery(IDbCommand comm);

		/// <summary>
		///     Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <returns></returns>
		string ConvertParameter(DbType type);
	}
}