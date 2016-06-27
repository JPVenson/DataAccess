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
		private string _currentIdent;

		public DatabaseObjectSelector(IQueryBuilder database, string currentIdent) : base(database)
		{
			_currentIdent = currentIdent;
		}

		public DatabaseObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		public DatabaseObjectSelector Alias(string alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			return new DatabaseObjectSelector(this, alias);
		}

		public SelectQuery<TPoco> Table<TPoco>(params object[] argumentsForFactory)
		{
			if (argumentsForFactory == null)
				throw new ArgumentNullException("argumentsForFactory");
			var cmd = ContainerObject.AccessLayer.CreateSelectQueryFactory(this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)), argumentsForFactory);
			return new SelectQuery<TPoco>(this.QueryCommand(cmd), null);
		}

		public ColumnChooser<TPoco> Only<TPoco>()
		{
			if (_currentIdent == null)
				_currentIdent = string.Format("{0}_{1}", base.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName, base.ContainerObject.GetNextParameterId());
			return new ColumnChooser<TPoco>(this, new List<string>(), _currentIdent);
		}
	}
}
