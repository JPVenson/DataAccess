using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Contracts;

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
