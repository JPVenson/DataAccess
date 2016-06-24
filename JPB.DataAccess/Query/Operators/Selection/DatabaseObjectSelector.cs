using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Selection
{
	public class DatabaseObjectSelector : QueryBuilderX, IDbElementSelector
	{
		public DatabaseObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		public SelectQuery<TPoco> Table<TPoco>(params object[] argumentsForFactory)
		{
			var cmd = ContainerObject.AccessLayer.CreateSelectQueryFactory(this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)), argumentsForFactory);
			return new SelectQuery<TPoco>(this.QueryCommand(cmd));
		}
	}
}
