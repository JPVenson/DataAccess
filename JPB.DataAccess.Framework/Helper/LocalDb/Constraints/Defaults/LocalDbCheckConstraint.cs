#region

using System;
using JPB.DataAccess.Framework.Contacts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Defaults
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbCheckConstraint{TEntity}" />
	public class LocalDbCheckConstraint<TEntity> : ILocalDbCheckConstraint<TEntity>
	{
		private Func<TEntity, bool> _constraint;

		private string _name;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbCheckConstraint{TEntity}" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="constraint">The constraint.</param>
		public LocalDbCheckConstraint(string name, Func<TEntity, bool> constraint)
		{
			_name = name;
			_constraint = constraint;
		}

		/// <summary>
		///     The name of this Constraint
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		///     The function that checks if the certain constraint is fulfilled
		/// </summary>
		/// <param name="item"></param>
		/// <returns>
		///     True if success false if failed
		/// </returns>
		public bool CheckConstraint(TEntity item)
		{
			try
			{
				return _constraint(item);
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}