using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.Core.Models;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface ITableInformations
	{
		string TableName { get; set; }
	}

	public interface IStoredProcedureInformation : ITableInformations
	{
		IEnumerable<SpParam> ParamaterSpParams { get; }
	}
}