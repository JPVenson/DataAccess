using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///		Allows for Modifications of Commands
	/// </summary>
	public interface ICommandProcessor
	{
		/// <summary>
		///		Should execute the operation and returns one or multiple resultsets
		/// </summary>
		/// <param name="db"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		EagarDataRecord[][] ExecuteMARSCommand(DbAccessLayer db, IDbCommand command, out int recordsAffected);

		///  <summary>
		/// 		Should execute the operation within a task and call onRecord each time a new Record is produced
		///  </summary>
		///  <param name="db"></param>
		///  <param name="command"></param>
		///  <param name="onRecord"></param>
		///  <param name="executionHint"></param>
		///  <returns></returns>
		Task EnumerateAsync(DbAccessLayer db, 
			IDbCommand command, 
			Action<IDataReader> onRecord,
			CommandBehavior executionHint = CommandBehavior.Default);
		
		void Enumerate(DbAccessLayer db, 
			IDbCommand command, 
			Action<IDataReader> onRecord,
			CommandBehavior executionHint = CommandBehavior.Default);

		Task<int> ExecuteCommandAsync(DbAccessLayer db, IDbCommand command);
		int ExecuteCommand(DbAccessLayer db, IDbCommand command);
		object GetSkalar(DbAccessLayer db, IDbCommand command, Type requestedType);
	}
}