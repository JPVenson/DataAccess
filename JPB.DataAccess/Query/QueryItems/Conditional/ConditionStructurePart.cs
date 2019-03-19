using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ConditionStructurePart : IConditionPart
	{
		internal enum LogicalOperator
		{
			And,
			Or
		}

		public ConditionStructurePart(LogicalOperator logOperator)
		{
			LogOperator = logOperator;
		}

		public LogicalOperator LogOperator { get; set; }

		public void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> next)
		{
			builder.QueryBuilder.Append($" {LogOperator.ToString().ToUpper()} ");
		}
	}
}