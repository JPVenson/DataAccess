#region

using System;
using System.Collections;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IConditionalOperatorQuery{TPoco}" />
	public class ConditionalOperatorQuery<TPoco> : QueryBuilderX, IConditionalOperatorQuery<TPoco>
	{
#pragma warning disable 1591
		public readonly CondtionBuilderState State;
#pragma warning restore 1591

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalOperatorQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="state">The state.</param>
		public ConditionalOperatorQuery(IQueryBuilder builder, CondtionBuilderState state) : base(builder)
		{
			State = state;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ConditionalOperatorQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public ConditionalOperatorQuery(IQueryBuilder builder) : base(builder)
		{
			State = new CondtionBuilderState(null);
			if (builder is ConditionalOperatorQuery<TPoco>)
			{
				State = ((ConditionalOperatorQuery<TPoco>)builder).State;
			}
		}

		/// <summary>
		///     Defines an condition that should be inverted
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Not
		{
			get { return new ConditionalOperatorQuery<TPoco>(this, State.ToOperator(Operator.Not)); }
		}

		/// <summary>
		///		Adds an Null check
		/// </summary>
		public ConditionalEvalQuery<TPoco> Null
		{
			get
			{
				switch (State.Operator)
				{
					case Operator.Is:
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"));
					case Operator.Not:
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NOT NULL"));
					default:
						throw new NotSupportedException("Invalid value");
				}
			}
		}

		/// <summary>
		///     Adds an IN or NOT BEWEEN statement for the given collection of values
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
			var prefix = "";
			switch (State.Operator)
			{
				case Operator.Is:
					prefix = "IN";
					break;
				case Operator.Not:
					prefix = "NOT IN";
					break;
				default:
					throw new NotSupportedException("Invalid value");
			}

			var prefixElement = this.QueryText(prefix + "(");

			foreach (var value in values)
			{
				var valId = prefixElement.ContainerObject.GetNextParameterId();
				prefixElement = prefixElement.QueryQ(string.Format("@m_val{0},", valId),
					new QueryParameter(string.Format("@m_val{0}", valId), value));
			}

			prefixElement.ContainerObject.Parts.Last().Prefix = prefixElement.ContainerObject.Parts.Last()
				.Prefix.Replace(",", "");

			prefixElement = prefixElement.QueryText(")");
			return new ConditionalEvalQuery<TPoco>(prefixElement, State);
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
			var prefix = "";
			switch (State.Operator)
			{
				case Operator.Is:
					prefix = "BETWEEN";
					break;
				case Operator.Not:
					prefix = "NOT BETWEEN";
					break;
				default:
					throw new NotSupportedException("Invalid value");
			}

			var valAId = ContainerObject.GetNextParameterId();
			var valBId = ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{0} @m_val{1} AND @m_val{2}", prefix, valAId, valBId),
					new QueryParameter(string.Format("@m_val{0}", valAId), valueA),
					new QueryParameter(string.Format("@m_val{0}", valBId), valueB)), State);
		}

		/// <summary>
		///     Creates a LIKE statement with an full whildcard %value%
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> Like(string value)
		{
			switch (State.Operator)
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
			switch (State.Operator)
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
			switch (State.Operator)
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
			switch (State.Operator)
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
			switch (State.Operator)
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
			switch (State.Operator)
			{
				case Operator.Is:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"), State);
					}
					return QueryOperatorValue("=", value);
				case Operator.Not:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NOT NULL"), State);
					}
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
			switch (State.Operator)
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
			switch (State.Operator)
			{
				case Operator.Is:
					return QueryOperatorValue("=", 0);
				case Operator.Not:
					return QueryOperatorValue("<>", 1);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Creates an Equal to statement by using a subquery
		/// </summary>
		/// <typeparam name="TGPoco">The type of the g poco.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Invalid value</exception>
		public ConditionalEvalQuery<TPoco> EqualsTo<TGPoco>(ElementProducer<TGPoco> value)
		{
			switch (State.Operator)
			{
				case Operator.Is:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"), State);
					}
					return QueryOperatorValue("=", value);
				case Operator.Not:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("NOT NULL"), State);
					}
					return QueryOperatorValue("<>", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

		/// <summary>
		///     Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue<TGPoco>(string operators, ElementProducer<TGPoco> sub)
		{
			var eval = this.QueryText(operators + "(");
			foreach (var genericQueryPart in sub.ContainerObject.Parts)
			{
				eval = eval.Add(genericQueryPart);
			}
			eval = this.QueryText(")");
			return new ConditionalEvalQuery<TPoco>(eval, State);
		}


		/// <summary>
		///     Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			var nextParameterId = ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{1} @m_val{0}", nextParameterId, operators),
					new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)), State);
		}
	}
}