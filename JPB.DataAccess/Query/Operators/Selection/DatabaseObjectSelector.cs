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
			ContainerObject.PostProcessors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Select, ContainerObject.AccessLayer));
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
				classInfo.TableName,
				classInfo,
				ContainerObject.CreateTableAlias(classInfo.TableName), 
				ContainerObject)));
		}

		/// <summary>
		///		Selects all columns from the given Identifier
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> Identifier<TPoco>(QueryIdentifier identifier)
		{
			ContainerObject.PostProcessors.Add(new EventPostProcessor(EventPostProcessor.EventType.Select, ContainerObject.AccessLayer));
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
				ContainerObject.Search(identifier),
				classInfo,
				ContainerObject.CreateTableAlias(classInfo.TableName), ContainerObject)));
		}
	}
}