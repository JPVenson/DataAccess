#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Selection
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IDbElementSelector" />
	public class DatabaseObjectSelector : QueryBuilderX, IDbElementSelector
	{
		private string _currentIdent;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="currentIdent">The current ident.</param>
		public DatabaseObjectSelector(IQueryBuilder database, string currentIdent) : base(database)
		{
			_currentIdent = currentIdent;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public DatabaseObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Creates a Select statement for a given Poco
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">argumentsForFactory</exception>
		public SelectQuery<TPoco> Table<TPoco>()
		{
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
				classInfo.TableName,
				classInfo,
				ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.Table))));
		}

		/// <summary>
		///		Selects all columns from the given Identifier
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> Identifier<TPoco>(QueryIdentifier identifier)
		{
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(ContainerObject.Search(identifier),
				ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)),
				ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.Table))));
		}
	}
}