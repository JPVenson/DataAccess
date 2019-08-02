using System;
using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ConditionStructurePart : IConditionPart
	{
		internal enum LogicalOperator
		{
			And,
			Or,
			OpenBracket,
			CloseBracket
		}

		public ConditionStructurePart(LogicalOperator logOperator)
		{
			LogOperator = logOperator;
		}

		public LogicalOperator LogOperator { get; set; }

		public void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> next)
		{
			switch (LogOperator)
			{
				case LogicalOperator.And:
				case LogicalOperator.Or:
					builder.QueryBuilder.Append($"{LogOperator.ToString().ToUpper()} ");
					break;
				case LogicalOperator.OpenBracket:
					builder.QueryBuilder.Append($"(");
					break;
				case LogicalOperator.CloseBracket:
					builder.QueryBuilder.Append($")");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}