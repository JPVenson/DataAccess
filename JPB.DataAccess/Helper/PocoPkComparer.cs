/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	/// Compares 2 Pocos based on there PrimaryKeys. Requires all Pocos to define one property with the PrimaryKey attribute
	/// When both of the instances are of the same reference: return true
	/// When one of the instances is default(T): return false
	/// When both of the instances Primary Key has the assertNotDatabaseMember: return false
	/// When both of the instances Primary Key are Equals: return true
	/// return false
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PocoPkComparer<T>
		: IEqualityComparer<T>, IComparer<T>
		where T : class
	{
		public static object DefaultAssertion;

		/// <summary>
		/// When Equals is used the result is stored in this Property
		/// </summary>
		public T Value { get; private set; }

		/// <summary>
		/// New Instance of the Auto Equality Comparer with no assertion on its default value for an Primary key
		/// </summary>
		public PocoPkComparer()
			: this(DefaultAssertion, DefaultAssertion != null)
		{
		}

		public PocoPkComparer(int assertNotDatabaseMember)
			: this((object)assertNotDatabaseMember, true)
		{

		}

		public PocoPkComparer(string assertNotDatabaseMember)
			: this((object)assertNotDatabaseMember, true)
		{

		}


		public PocoPkComparer(long assertNotDatabaseMember)
			: this((object)assertNotDatabaseMember, true)
		{

		}

		internal PocoPkComparer(object assertNotDatabaseMember, bool useAssertion = false, string propertyName = null)
		{
			if (assertNotDatabaseMember == null && DbAccessLayer.DefaultAssertionObject != null)
			{
				assertNotDatabaseMember = DbAccessLayer.DefaultAssertionObject;
				useAssertion = true;
			}
			Init(assertNotDatabaseMember, useAssertion, propertyName);
		}

		internal static void Visit(ref Expression left, ref Expression right)
		{
			var leftTypeCode = Type.GetTypeCode(left.Type);
			var rightTypeCode = Type.GetTypeCode(right.Type);

			if (leftTypeCode == rightTypeCode)
				return;

			if (leftTypeCode > rightTypeCode)
				right = Expression.Convert(right, left.Type);
			else
				left = Expression.Convert(left, right.Type);
		}

		private void Init(object assertNotDatabaseMember, bool useAssertion, string property = null)
		{
			var plainType = typeof(T);

			_typeInfo = plainType.GetClassInfo();
			if (_typeInfo.PrimaryKeyProperty == null)
				throw new NotSupportedException(string.Format("The type '{0}' does not define any PrimaryKey", plainType.Name));

			if (string.IsNullOrEmpty(property))
				property = _typeInfo.PrimaryKeyProperty.PropertyName;

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
					if (DbAccessLayer.DefaultAssertionObjectRewrite)
					{
						Visit(ref PropLeft, ref assertionObject);
					}
					else
					{
						throw new NotSupportedException(string.Format("Unknown Type cast detected." +
																	  " Assert typ is '{0}' property is '{1}' " +
																	  "... sry i am good but not as this good! Try the DbAccessLayer.DefaultAssertionObjectRewrite option", assertNotDatabaseMember.GetType().Name, _typeInfo.PrimaryKeyProperty.PropertyType.Name));
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

			if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
			{
				var directComparsion = Expression.Call(Left, "CompareTo", null, Right);
				_compareTo = Expression.Lambda<Func<T, T, int>>(directComparsion, new[] { Left, Right }).Compile();
			}
		}

		internal ParameterExpression Left;
		internal ParameterExpression Right;
		internal Expression PropLeft;
		internal LabelTarget ReturnTarget;

		private Func<T, T, bool> Wrap(ConditionalExpression exp)
		{
			return Expression.Lambda<Func<T, T, bool>>(Expression.Block(
				exp,
				Expression.Label(ReturnTarget, Expression.Constant(false))
				), new[]
				{
					Left,Right
				}).Compile();
		}

		private DbClassInfoCache _typeInfo;
		private Func<T, T, bool> _assertionBlock;
		private Func<T, T, bool> _propEqual;
		private Func<T, T, int> _compareTo;

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
				return false;
			if (rightNull)
				return false;

			if (_assertionBlock != null && _assertionBlock(left, right))
				return false;
			if (_propEqual(left, right))
			{
				Value = left;
				return true;
			}
			return false;
		}

		public int GetHashCode(T obj)
		{
			return obj.GetHashCode();
		}

		public int Compare(T left, T right)
		{
			var leftNull = left == null;
			var rightNull = right == null;
			//generic Checks
			if (ReferenceEquals(left, right))
				return 0;

			if (leftNull)
				return -1;
			if (rightNull)
				return 1;
			return _compareTo(left, right);
		}
	}
}
