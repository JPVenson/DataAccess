using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal interface IConditionPart
	{
		void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> parts);
	}
}