using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IElementComparer<T>
	{
		IEnumerable<string> Compare(T other);
	}
}