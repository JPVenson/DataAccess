using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IEnumDeclarationModel
	{
		Dictionary<int, string> Values { get; }
		string Name { get; set; }
	}
}