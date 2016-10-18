using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Selection
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IDbElementSelector" />
	public class DatabaseObjectSelector : QueryBuilderX, IDbElementSelector
	{
		private string _currentIdent;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseObjectSelector"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="currentIdent">The current ident.</param>
		public DatabaseObjectSelector(IQueryBuilder database, string currentIdent) : base(database)
		{
			_currentIdent = currentIdent;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseObjectSelector"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public DatabaseObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		/// Changes the current Identitfyer
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">alias</exception>
		public DatabaseObjectSelector Alias(string alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			return new DatabaseObjectSelector(this, alias);
		}

		/// <summary>
		/// Creates a Select statement for a given Poco
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="argumentsForFactory">The arguments for factory.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">argumentsForFactory</exception>
		public SelectQuery<TPoco> Table<TPoco>(params object[] argumentsForFactory)
		{
			if (argumentsForFactory == null)
				throw new ArgumentNullException("argumentsForFactory");
			var cmd = ContainerObject.AccessLayer.CreateSelectQueryFactory(this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)), argumentsForFactory);
			return new SelectQuery<TPoco>(this.QueryCommand(cmd), null);
		}

		/// <summary>
		/// Creates an Column Chooser object to spezify columns to select
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public ColumnChooser<TPoco> Only<TPoco>()
		{
			if (_currentIdent == null)
				_currentIdent = string.Format("{0}_{1}", base.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName, base.ContainerObject.GetNextParameterId());
			return new ColumnChooser<TPoco>(this, new List<string>(), _currentIdent);
		}
	}
}
