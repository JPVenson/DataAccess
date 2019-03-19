using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ConditionStatementQueryPart : IQueryPart
	{
		public ConditionStatementQueryPart()
		{
			Conditions = new List<IConditionPart>();
		}

		public List<IConditionPart> Conditions { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var builder = new ConditionalQueryBuilder();
			builder.QueryBuilder.Append("WHERE ");

			var conditions = new Queue<IConditionPart>(Conditions);
			while (conditions.Any())
			{
				conditions.Dequeue().Render(builder, conditions);
			}

			return container.AccessLayer.Database
				.CreateCommandWithParameterValues(builder.QueryBuilder.ToString(), builder.QueryParameters);
		}
	}
}