using System;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class ConditionalOperatorQuery<TPoco> : QueryBuilderX, IConditionalOperatorQuery<TPoco>
	{
		public readonly CondtionBuilderState State;

		public ConditionalOperatorQuery(IQueryBuilder builder, CondtionBuilderState state) : base(builder)
		{
			State = state;
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Not()
		{
			return new ConditionalOperatorQuery<TPoco>(this, State.ToOperator(Operator.Not));
		}

		public ConditionalEvalQuery<TPoco> In<TValue>(params TValue[] values)
		{
			var prefix = "";
			switch (State.Operator)
			{
				case Operator.Is:
					prefix = "IN";
					break;
				case Operator.Not:
					prefix = "NOT BETWEEN";
					break;
				default:
					throw new NotSupportedException("Invalid value");
			}

			var prefixElement = this.QueryText(prefix + "(");

			foreach (var value in values)
			{
				var valId = this.ContainerObject.GetNextParameterId();
				prefixElement = this.QueryQ(string.Format("@m_val{0},", valId), new QueryParameter(string.Format("@m_val{0}", valId), value));
			}

			prefixElement.ContainerObject.Parts.Last().Prefix = prefixElement.ContainerObject.Parts.Last().Prefix.Replace(",", "");

			prefixElement = prefixElement.QueryText(")");
			return new ConditionalEvalQuery<TPoco>(prefixElement, State);
		}

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

			var valAId = this.ContainerObject.GetNextParameterId();
			var valBId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{0} @m_val{1} AND @m_val{2}", prefix, valAId, valBId),
				new QueryParameter(string.Format("@m_val{0}", valAId), valueA),
				new QueryParameter(string.Format("@m_val{0}", valBId), valBId)), State);
		}

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

		public ConditionalEvalQuery<TPoco> EndWith(string value)
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

		public ConditionalEvalQuery<TPoco> StartWith(string value)
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
						return new ConditionalEvalQuery<TPoco>(this.QueryText("NOT NULL"), State);
					}
					return QueryOperatorValue("<>", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}

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
		/// Prepaires an Conditional Query
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
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{1} @m_val{0}", nextParameterId, operators), new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)), State);
		}
	}
}
