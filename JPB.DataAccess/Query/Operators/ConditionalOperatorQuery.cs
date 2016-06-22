using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public enum Operator
	{
		Is,
		Not
	}

	public class ConditionalOperatorQuery<TPoco> : QueryBuilderX, IConditionalOperatorQuery<TPoco>
	{
		private readonly Operator _isOrNot;

		public ConditionalOperatorQuery(IQueryBuilder builder, Operator isOrNot) : base(builder)
		{
			_isOrNot = isOrNot;
		}

		/// <summary>
		/// Defines an condition that should be inverted
		/// </summary>
		public ConditionalOperatorQuery<TPoco> Not()
		{
			return new ConditionalOperatorQuery<TPoco>(this, Operator.Not);
		}

		public ConditionalEvalQuery<TPoco> Like(string value)
		{
			switch (_isOrNot)
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
			switch (_isOrNot)
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
			switch (_isOrNot)
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
			switch (_isOrNot)
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
			switch (_isOrNot)
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
			switch (_isOrNot)
			{
				case Operator.Is:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("IS NULL"));
					}
					return QueryOperatorValue("=", value);
				case Operator.Not:
					if (value == null)
					{
						return new ConditionalEvalQuery<TPoco>(this.QueryText("NOT NULL"));
					}
					return QueryOperatorValue("<>", value);
				default:
					throw new NotSupportedException("Invalid value");
			}
		}


		/// <summary>
		/// Prepaires an Conditional Query
		/// </summary>
		public ConditionalEvalQuery<TPoco> QueryOperatorValue(string operators, object value)
		{
			var nextParameterId = this.ContainerObject.GetNextParameterId();
			return new ConditionalEvalQuery<TPoco>(this
				.QueryQ(string.Format("{1} @m_val{0}", nextParameterId, operators), new QueryParameter(string.Format("@m_val{0}", nextParameterId), value)));
		}
	}
}
