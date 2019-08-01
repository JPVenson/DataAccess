using System.Collections.Generic;

namespace JPB.DataAccess.Framework.Query.QueryItems.Conditional
{
	internal interface IConditionPart
	{
		void Render(ConditionalQueryBuilder builder, Queue<IConditionPart> parts);
	}
}