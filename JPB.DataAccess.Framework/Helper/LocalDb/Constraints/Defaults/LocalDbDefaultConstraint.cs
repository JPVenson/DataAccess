#region

using System;
using JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Defaults
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="ILocalDbDefaultConstraint{TEntity}" />
	public class LocalDbDefaultConstraint<TEntity, TValue> : ILocalDbDefaultConstraint<TEntity>
	{
		private readonly TValue _value;
		private Action<TEntity, TValue> _set;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbDefaultConstraint{TEntity, TValue}" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="set">The set.</param>
		/// <exception cref="ArgumentNullException">
		///     name
		///     or
		///     value
		///     or
		///     set
		/// </exception>
		public LocalDbDefaultConstraint(string name, TValue value, Action<TEntity, TValue> set)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if (set == null)
			{
				throw new ArgumentNullException(nameof(set));
			}
			_value = value;
			_set = set;
			Name = name;
		}

		/// <summary>
		///     The name of this Constraint
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     Defaults the value.
		/// </summary>
		/// <param name="item">The item.</param>
		public void DefaultValue(TEntity item)
		{
			_set(item, _value);
		}
	}
}