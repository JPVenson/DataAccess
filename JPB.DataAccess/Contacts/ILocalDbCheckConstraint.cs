using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	/// Creates a new Strong Typed Constraint
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.ILocalDbConstraint" />
	public interface ILocalDbCheckConstraint<in TEntity> : ILocalDbConstraint
	{


		/// <summary>
		/// The function that checks if the certain constraint is fulfilled
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if success false if failed</returns>
		bool CheckConstraint(TEntity item);
	}

	/// <summary>
	/// Defines a new Constraint that can be applyed to a Database
	/// </summary>
	public interface ILocalDbConstraint
	{
		/// <summary>
		/// The name of this Constraint
		/// </summary>
		string Name { get; }
	}
}
