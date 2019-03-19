#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IConditionalOperatorQuery{TPoco}" />
	public class ConditionalOperatorQuery<TPoco> : IConditionalOperatorQuery<TPoco>
	{
		private readonly IQueryBuilder _builder;
		private readonly ExpressionConditionPart _expression;

		/// <summary>
		/// 
		/// </summary>
		public readonly Operator State;

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalOperatorQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="expression"></param>
		/// <param name="state">The state.</param>
		internal ConditionalOperatorQuery(IQueryBuilder builder, ExpressionConditionPart expression, Operator state)
		{
			_builder = builder;
			_expression = expression;
			State = state;
		}

		/// <summary>
		///     Defines an condition that should be inverted
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Not
		{
			get { return new ConditionalOperatorQuery<TPoco>(_builder, _expression, Operator.Not); }
		}

		/// <summary>
		///		Adds an Null check
		/// </summary>
		public ConditionalEvalQuery<TPoco> Null
		{
			get
			{
				switch (State)
				{
					case Operator.Is:
						_expression.Operator = "IS";
						_expression.Value = "NULL";
						break;
					case Operator.Not:
						_expression.Operator = "IS NOT";
						_expression.Value = "NULL";
						break;
					default:
						throw new NotSupportedException("Invalid value");
				}
				return new ConditionalEvalQuery<TPoco>(_builder);
			}
		}

		/// <summary>
		///     Adds an IN or NOT BETWEEN statement for the given collection of values
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> In<TValue>(params TValue[] values)
		{
			if (values.Length == 0)
			{
				throw new NotSupportedException("Cannot add no values to In");
			}
			if (typeof(TValue).IsAssignableFrom(typeof(IEnumerable)))
			{
				throw new InvalidOperationException(
				"Enumerations are not allowed. Did you forgot to call ToArray on the Linq expression?");
			}

			switch (State)
			{
				case Operator.Is:
					_expression.Operator = "IN";
					break;
				case Operator.Not:
					_expression.Operator = "NOT IN";
					break;
				default:
					throw new NotSupportedException("Invalid value");
			}

			var arguments = new List<IQueryParameter>();
			foreach (var value in values)
			{
				var valId = _builder.ContainerObject.GetNextParameterId();
				arguments.Add(new QueryParameter($"@m_val{valId}", value));
			}
			_expression.Value = new ExpressionValue($"({arguments.Select(e => e.Name).Aggregate((e, f) => e + "," + f)})", arguments.ToArray());
			return new ConditionalEvalQuery<TPoco>(_builder);
		}

		/// <summary>
		///     Creates a BETWEEN or NOT BETWEEN statement for <paramref name="valueA" /> and <paramref name="valueB" />
		/// </summary>
		/// <param name="valueA">The value a.</param>
		/// <param name="valueB">The value b.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> Between(object valueA, object valueB)
		{
			switch (State)
			{
				case Operator.Is:
					_expression.Operator = "BETWEEN";
					break;
				case Operator.Not:
					_expression.Operator = "NOT BETWEEN";
					break;
				default:
					throw new NotSupportedException("Invalid value");
			}

			var valAId = new QueryParameter($"@m_val{_builder.ContainerObject.GetNextParameterId()}", valueA);
			var valBId = new QueryParameter($"@m_val{_builder.ContainerObject.GetNextParameterId()}", valueB);

			_expression.Value = new ExpressionValue($"{valAId.Name} AND {valBId.Name}", valAId, valBId);

			return new ConditionalEvalQuery<TPoco>(_builder);
		}

		/// <summary>
		///     Creates a LIKE statement with an full whildcard %value%
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> Like(string value)
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("LIKE ", "%" + value + "%");
				case Operator.Not:
					return QueryOperatorValue("NOT LIKE", "%" + value + "%");
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a LIKE statement with an ending whildcard value%
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> StartWith(string value)
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("LIKE ", value + "%");
				case Operator.Not:
					return QueryOperatorValue("NOT LIKE", value + "%");
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a LIKE statement with a starting whildcard %value
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> EndWith(string value)
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("LIKE ", "%" + value);
				case Operator.Not:
					return QueryOperatorValue("NOT LIKE", "%" + value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a Conditonal Bigger or Smaller as Statement
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> BiggerThen(object value)
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue(">", value);
				case Operator.Not:
					return QueryOperatorValue("<", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a Conditonal Bigger or Smaller as Statement
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> SmallerThen(object value)
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("<", value);
				case Operator.Not:
					return QueryOperatorValue(">", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a statement that will check the Column for equality or not
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> EqualsTo(object value)
		{
			if (value == null)
			{
				return Null;
			}

			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("=", value);
				case Operator.Not:
					return QueryOperatorValue("<>", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a statement that will check the Column for equality or not
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> True()
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("=", 1);
				case Operator.Not:
					return QueryOperatorValue("<>", 0);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates a statement that will check the Column for equality or not
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> False()
		{
			switch (State)
			{
				case Operator.Is:
					return QueryOperatorValue("=", 0);
				case Operator.Not:
					return QueryOperatorValue("<>", 1);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		///// <summary>
		/////     Creates an Equal to statement by using a subquery
		///// </summary>
		///// <typeparam name="TGPoco">The type of the g poco.</typeparam>
		///// <param name="value">The value.</param>
		///// <returns></returns>
		///// <exception cref="NotSupportedException">Invalid value</exception>
		//public ConditionalEvalQuery<TPoco> EqualsTo<TGPoco>(ElementProducer<TGPoco> value)
		//{
		//	if (value == null)
		//	{
		//		return Null;
		//	}
		//	switch (State)
		//	{
		//		case Operator.Is:
		//			return QueryOperatorValue("=", value);
		//		case Operator.Not:
		//			return QueryOperatorValue("<>", value);
		//		default:
		//			throw new NotSupportedException("Invalid value");
		//	}
		//}

		///// <summary>
		/////     Prepaires an Conditional Query
		///// </summary>
		//public ConditionalEvalQuery<TPoco> QueryOperatorValue<TGPoco>(string operators, ElementProducer<TGPoco> sub)
		//{
		//	var eval = this.QueryText(operators + "(");
		//	foreach (var genericQueryPart in sub.ContainerObject.Parts)
		//	{
		//		eval = eval.Add(genericQueryPart);
		//	}
		//	eval = this.QueryText(")");
		//	return new ConditionalEvalQuery<TPoco>(eval);
		//}


		/// <summary>
		///     Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			_expression.Operator = operators;
			var valueParameter = new QueryParameter($"@val_{_builder.ContainerObject.GetNextParameterId()}", value);
			_expression.Value = new ExpressionValue(valueParameter.Name, valueParameter);
			return new ConditionalEvalQuery<TPoco>(_builder);
		}
	}
}