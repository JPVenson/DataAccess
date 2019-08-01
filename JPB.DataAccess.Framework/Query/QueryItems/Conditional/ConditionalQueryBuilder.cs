using System.Collections.Generic;
using System.Text;
using JPB.DataAccess.Framework.Contacts;

namespace JPB.DataAccess.Framework.Query.QueryItems.Conditional
{
	internal class ConditionalQueryBuilder
	{
		public ConditionalQueryBuilder()
		{
			QueryBuilder = new StringBuilder();
			QueryParameters = new List<IQueryParameter>();
		}

		public StringBuilder QueryBuilder { get; set; }
		public List<IQueryParameter> QueryParameters { get; private set; }
	}
}