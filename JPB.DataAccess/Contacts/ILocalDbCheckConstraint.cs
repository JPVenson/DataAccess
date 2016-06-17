using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Contacts
{
	public interface ILocalDbCheckConstraint : ILocalDbConstraint
	{


		/// <summary>
		/// The function that checks if the certain constraint is fulfilled
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if success false if failed</returns>
		bool CheckConstraint(object item);
	}

	public interface ILocalDbConstraint
	{
		/// <summary>
		/// The name of this Constraint
		/// </summary>
		string Name { get; }
	}
}
