using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Framework.Query.QueryItems.Conditional
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