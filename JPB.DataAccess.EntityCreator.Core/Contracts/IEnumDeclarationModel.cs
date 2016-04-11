using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IEnumDeclarationModel
	{
		Dictionary<int, string> Values { get; }
		string Name { get; set; }
	}
}