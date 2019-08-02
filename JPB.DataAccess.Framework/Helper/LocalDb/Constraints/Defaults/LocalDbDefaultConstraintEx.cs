#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Defaults
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="ILocalDbDefaultConstraint{TEntity}" />
	public class LocalDbDefaultConstraintEx<TEntity, TValue> : ILocalDbDefaultConstraint<TEntity>
	{
		private readonly DbConfig _config;
		private readonly Expression<Func<TEntity, TValue>> _exp;
		private readonly Func<TValue> _generateValue;
		private DbPropertyInfoCache _dbPropertyInfoCache;

		/// <summary>
		///     Initializes a new instance of the <see cref="LocalDbDefaultConstraintEx{TEntity, TValue}" /> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="name">The name.</param>
		/// <param name="generateValue">The generate value.</param>
		/// <param name="column">The column.</param>
		/// <exception cref="ArgumentException">
		/// </exception>
		/// <exception cref="InvalidCastException">
		///     The given property name is invalid. When using Nullable types do not use the
		///     Value property. Use the Nullable propertie
		/// </exception>
		public LocalDbDefaultConstraintEx(DbConfig config, string name, Func<TValue> generateValue,
			Expression<Func<TEntity, TValue>> column)
		{
			_config = config;
			_generateValue = generateValue;
			_exp = column;
			Name = name;

			var member = _exp.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a method, not a property.",
				_exp));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a field, not a property.",
				_exp));
			}

			var type = _config.GetOrCreateClassInfoCache(typeof(TEntity));

			var fod = type.Propertys.FirstOrDefault(f => f.Key == propInfo.Name);

			if (fod.Value == null)
			{
				throw new InvalidCastException(
				"The given property name is invalid. When using Nullable types do not use the Value property. Use the Nullable propertie");
			}

			_dbPropertyInfoCache = fod.Value;
		}

		/// <summary>
		///     Defaults the value.
		/// </summary>
		/// <param name="item">The item.</param>
		public void DefaultValue(TEntity item)
		{
			var value = _generateValue();
			var preValue = _dbPropertyInfoCache.Getter.Invoke(item);
			var defaultValue = default(TValue);
			if (defaultValue == null && preValue == null || defaultValue != null && defaultValue.Equals(preValue))
			{
				_dbPropertyInfoCache.Setter.Invoke(item, value);
			}
		}

		/// <summary>
		///     The name of this Constraint
		/// </summary>
		public string Name { get; private set; }
	}
}