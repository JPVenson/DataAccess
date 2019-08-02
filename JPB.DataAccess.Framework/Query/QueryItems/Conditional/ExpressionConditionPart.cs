using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ExpressionConditionPart : IConditionPart
	{
		public ExpressionConditionPart(ColumnInfo columnName)
		{
			Column = columnName;
		}

		public ColumnInfo Column { get; set; }
		public string Operator { get; set; }
		public ExpressionValue Value { get; set; }

		public void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> next)
		{
			builder.QueryParameters.AddRange(Value.QueryParameters);
			builder.QueryBuilder.Append($" {Column.ColumnSourceAlias()} {Operator} {Value.QueryValue} ");
		}
	}
}