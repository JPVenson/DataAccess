using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ExpressionConditionPart : IConditionPart
	{
		private readonly QueryIdentifier _currentAlias;

		public ExpressionConditionPart(QueryIdentifier currentAlias, string columnName)
		{
			_currentAlias = currentAlias;
			Column = columnName;
		}

		public string Column { get; set; }
		public string Operator { get; set; }
		public ExpressionValue Value { get; set; }

		public void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> next)
		{
			builder.QueryParameters.AddRange(Value.QueryParameters);
			builder.QueryBuilder.Append($" {DbAccessLayerHelper.ColumnIdentifier(_currentAlias.Value, Column)} {Operator} {Value.QueryValue} ");
		}
	}
}