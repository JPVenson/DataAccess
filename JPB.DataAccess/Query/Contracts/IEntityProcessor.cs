using System;
using System.Data;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///		Allows Modifications on Entities mapping
	/// </summary>
	public interface IEntityProcessor
	{
		/// <summary>
		///		Will be invoked right before execution of the command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		IDbCommand BeforeExecution(IDbCommand command);

		/// <summary>
		///		Transforms an Entity
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		object Transform(object entity, Type entityType, QueryProcessingEntitiesContext context);

		/// <summary>
		///		Transforms an DataReader
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		EagarDataRecord Transform(EagarDataRecord reader, Type entityType, QueryProcessingRecordsContext context);

		/// <summary>
		///		Transforms all DataReaders
		/// </summary>
		/// <param name="readers"></param>
		/// <param name="entityType"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context);
	}
}