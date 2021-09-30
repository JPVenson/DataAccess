using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	public class SharedInterface : ISharedInterface
	{
		public SharedInterface(string name, ISharedInterface parent, IList<IColumInfoModel> containsColumns)
		{
			Name = name;
			Parent = parent;
			ContainsColumns = containsColumns;
		}

		public ISharedInterface Parent { get; set; }
		public IList<IColumInfoModel> ContainsColumns { get; private set; }
		public string Name { get; set; }
	}
}
