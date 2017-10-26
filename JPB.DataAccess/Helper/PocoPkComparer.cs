#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;

#endregion

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///     Compares 2 Pocos based on there PrimaryKeys. Requires all Pocos to define one property with the PrimaryKey
	///     attribute
	///     When both of the instances are of the same reference: return true
	///     When one of the instances is default(T): return false
	///     When both of the instances Primary Key has the assertNotDatabaseMember: return false
	///     When both of the instances Primary Key are Equals: return true
	///     return false
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PocoPkComparer<T>
		: IEqualityComparer<T>, IComparer<T>
		where T : class
	{
		/// <summary>
		///     The default assertion object that will be used to compare to when no other element is used
		/// </summary>
		// ReSharper disable once StaticMemberInGenericType
		public static object DefaultAssertionObject;

		/// <summary>
		///     Should a cast tried
		/// </summary>
		// ReSharper disable once StaticMemberInGenericType
		public static bool DefaultRewrite;

		private Func<T, T, bool> _assertionBlock;
		private Func<T, T, int> _compareTo;
		private Func<T, int> _getHashCodeOfProp;
		private Func<T, T, bool> _propEqual;

		private DbClassInfoCache _typeInfo;

		internal ParameterExpression Left;
		internal Expression PropLeft;
		internal LabelTarget ReturnTarget;
		internal ParameterExpression Right;

		/// <summary>
		///     New Instance of the Auto Equality Comparer with no assertion on its default value for an Primary key
		/// </summary>
		public PocoPkComparer()
			: this(DefaultAssertionObject, new DbConfig(true), DefaultAssertionObject != null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		public PocoPkComparer(int assertNotDatabaseMember)
			: this(assertNotDatabaseMember, new DbConfig(true), true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		public PocoPkComparer(string assertNotDatabaseMember)
			: this(assertNotDatabaseMember, new DbConfig(true), true)
		{
		}


		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		public PocoPkComparer(long assertNotDatabaseMember)
			: this(assertNotDatabaseMember, new DbConfig(true), true)
		{
		}

		/// <summary>
		///     New Instance of the Auto Equality Comparer with no assertion on its default value for an Primary key
		/// </summary>
		public PocoPkComparer(DbConfig config)
			: this(DefaultAssertionObject, config, DefaultAssertionObject != null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		/// <param name="config">The configuration.</param>
		public PocoPkComparer(int assertNotDatabaseMember, DbConfig config)
			: this(assertNotDatabaseMember, config, true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		/// <param name="config">The configuration.</param>
		public PocoPkComparer(string assertNotDatabaseMember, DbConfig config)
			: this(assertNotDatabaseMember, config, true)
		{
		}


		/// <summary>
		///     Initializes a new instance of the <see cref="PocoPkComparer{T}" /> class.
		/// </summary>
		/// <param name="assertNotDatabaseMember">The assert not database member.</param>
		/// <param name="config">The configuration.</param>
		public PocoPkComparer(long assertNotDatabaseMember, DbConfig config)
			: this(assertNotDatabaseMember, config, true)
		{
		}

		internal PocoPkComparer(
			object assertNotDatabaseMember,
			DbConfig config,
			bool useAssertion = false,
			string propertyName = null)
		{
			if (assertNotDatabaseMember == null && DefaultAssertionObject != null)
			{
				assertNotDatabaseMember = DefaultAssertionObject;
				useAssertion = true;
			}
			Init(assertNotDatabaseMember, useAssertion, config, propertyName);
		}

		/// <summary>
		///     When Equals is used the result is stored in this Property
		/// </summary>
		public T Value { get; private set; }

		/// <summary>
		///     Checks if both arguments are ReferenceEquals
		///     Checks if Left is null = 1
		///     Checks if Right is null = -1
		///     Calls the
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public int Compare(T left, T right)
		{
			var leftNull = left == null;
			var rightNull = right == null;
			//generic Checks
			if (ReferenceEquals(left, right))
			{
				return 0;
			}

			if (leftNull)
			{
				return 1;
			}
			if (rightNull)
			{
				return -1;
			}

			if (_compareTo == null)
			{
				throw new NotSupportedException(string.Format("The Primary key on object '{0}' does not implement IComparable",
				_typeInfo.Name));
			}
			return _compareTo(left, right);
		}

		/// <summary>
		///     Checks if both have the same Reference.
		///     Checks if any but not both of them are null.
		///     Compares both Primary keys against the assertNotDatabaseMember Object
		///     Compares both Primary key Propertys by using Equals
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public bool Equals(T left, T right)
		{
			var leftNull = left == null;
			var rightNull = right == null;
			//generic Checks
			if (ReferenceEquals(left, right))
			{
				Value = left;
				return true;
			}

			if (leftNull)
			{
				return false;
			}
			if (rightNull)
			{
				return false;
			}

			if (_assertionBlock != null && _assertionBlock(left, right))
			{
				return false;
			}

			if (_propEqual(left, right))
			{
				Value = left;
				return true;
			}
			return false;
		}

		/// <summary>
		///     Calls the GetHashCode function on the PrimaryKey
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(T obj)
		{
			return _getHashCodeOfProp(obj);
		}

		internal static void Visit(ref Expression left, ref Expression right)
		{
			var leftTypeCode = Type.GetTypeCode(left.Type);
			var rightTypeCode = Type.GetTypeCode(right.Type);

			if (leftTypeCode == rightTypeCode)
			{
				return;
			}

			if (leftTypeCode > rightTypeCode)
			{
				right = Expression.Convert(right, left.Type);
			}
			else
			{
				left = Expression.Convert(left, right.Type);
			}
		}

		private void Init(
			object assertNotDatabaseMember,
			bool useAssertion,
			DbConfig config,
			string property = null)
		{
			var plainType = typeof(T);

			_typeInfo = config.GetOrCreateClassInfoCache(plainType);
			if (_typeInfo.PrimaryKeyProperty == null && string.IsNullOrEmpty(property))
			{
				throw new NotSupportedException(string.Format("The type '{0}' does not define any PrimaryKey", plainType.Name));
			}

			if (string.IsNullOrEmpty(property))
			{
				property = _typeInfo.PrimaryKeyProperty.PropertyName;
			}

			ReturnTarget = Expression.Label(typeof(bool));
			var returnTrue = Expression.Return(ReturnTarget, Expression.Constant(true));

			Left = Expression.Parameter(plainType);
			Right = Expression.Parameter(plainType);

			//left or right property null
			PropLeft = Expression.Property(Left, property);
			var propRight = Expression.Property(Right, property);
			ConditionalExpression resAssertionBlock = null;

			if (useAssertion)
			{
				Expression assertionObject = Expression.Constant(assertNotDatabaseMember);
				if (_typeInfo.PrimaryKeyProperty.PropertyType != assertNotDatabaseMember.GetType())
				{
					if (DefaultRewrite)
					{
						Visit(ref PropLeft, ref assertionObject);
					}
					else
					{
						throw new NotSupportedException(string.Format("Unknown Type cast detected." +
						                                              " Assert typ is '{0}' property is '{1}' " +
						                                              "... sry i am good but not as this good! Try the PocoPkComparer.DefaultRewrite option",
						assertNotDatabaseMember.GetType().Name, _typeInfo.PrimaryKeyProperty.PropertyType.Name));
					}
				}

				var eqLeftPropEqualsAssertion = Expression.Equal(PropLeft, assertionObject);
				var eqRightPropEqualsAssertion = Expression.Equal(propRight, assertionObject);
				var resLeftAndRightEqualsAssertion = Expression.And(eqLeftPropEqualsAssertion, eqRightPropEqualsAssertion);
				resAssertionBlock = Expression.IfThen(resLeftAndRightEqualsAssertion, returnTrue);
			}

			//equal
			var eqPropertyEqual = Expression.Equal(PropLeft, propRight);

			if (resAssertionBlock != null)
			{
				_assertionBlock = Wrap(resAssertionBlock);
			}
			_propEqual = Wrap(Expression.IfThen(eqPropertyEqual, returnTrue));

			if (typeof(IComparable).IsAssignableFrom(typeof(T)))
			{
				var directComparsion = Expression.Call(Left, "CompareTo", null, Right);
				_compareTo = Expression.Lambda<Func<T, T, int>>(directComparsion, Left, Right).Compile();
			}

			var returnhasCode = Expression.Label(typeof(int));

			var hashCode = Expression.Call(Left, "GetHashCode", null);
			_getHashCodeOfProp = Expression.Lambda<Func<T, int>>(Expression.Block(
				hashCode,
				Expression.Label(returnhasCode, Expression.Constant(-1))
			), Left).Compile();
		}

		private Func<T, T, bool> Wrap(Expression exp)
		{
			return Expression.Lambda<Func<T, T, bool>>(Expression.Block(
				exp,
				Expression.Label(ReturnTarget, Expression.Constant(false))
			), Left, Right).Compile();
		}
	}
}