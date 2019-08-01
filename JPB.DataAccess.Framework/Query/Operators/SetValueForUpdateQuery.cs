using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JPB.DataAccess.Framework.MetaApi;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems;

namespace JPB.DataAccess.Framework.Query.Operators
{
	/// <summary>
	///		An update related Column selection
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class SetValueForUpdateQuery<TPoco>
	{
		private readonly IQueryBuilder _queryBuilder;

		/// <inheritdoc />
		public SetValueForUpdateQuery(IQueryBuilder queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		/// <summary>
		///		Adds the Column name
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		[PublicAPI]
		public UpdateValueQuery<TPoco> Column(string columnName)
		{
			return new UpdateValueQuery<TPoco>(_queryBuilder, new UpdateTableWithQueryPart.ColumnAssignment()
			{
				Column = columnName
			});
		}
		/// <summary>
		///		Adds the Column name
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		[PublicAPI]
		public UpdateValueQuery<TPoco> Column<TA>([NoEnumeration] Expression<Func<TPoco, TA>> columnName)
		{
			return Column(
				_queryBuilder.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco))
					.Propertys[columnName.GetPropertyInfoFromLamdba()].DbName);
		}
	}
}