using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class ConditionStatementQueryPart : IQueryPart
	{
		public ConditionStatementQueryPart()
		{
			Conditions = new List<IConditionPart>();
		}

		public List<IConditionPart> Conditions { get; private set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var builder = new ConditionalQueryBuilder();
			builder.QueryBuilder.Append("WHERE ");

			var conditions = new Queue<IConditionPart>(Conditions);
			while (conditions.Any())
			{
				conditions.Dequeue().Render(builder, conditions);
			}
			return new QueryFactoryResult(builder.QueryBuilder.ToString(), builder.QueryParameters.ToArray());
		}
	}
}