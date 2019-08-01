using System.Linq;
using JetBrains.Annotations;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.Helper.LocalDb.Scopes;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.Operators.Conditional;
using JPB.DataAccess.Framework.Query.QueryItems;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Framework.Query.Operators
{
	/// <summary>
	///		Select a Target for an Update query
	/// </summary>
	public class PrepaireUpdateQuery : QueryBuilderX
	{
		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryBuilder database) : base(database)
		{
		}
		
		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create an update statement that will check for the id of the obj
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		[MustUseReturnValue]
		public ConditionalEvalQuery<TEntity> Entity<TEntity>(TEntity obj)
		{
			ContainerObject.Interceptors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Update, ContainerObject.AccessLayer));
			var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TEntity));
			var targetAlias = ContainerObject.CreateTableAlias(dbClassInfoCache.TableName);
			var queryIdentifier = new QueryIdentifier()
			{
				Value = dbClassInfoCache.TableName,
				QueryIdType = QueryIdentifier.QueryIdTypes.Table
			};

			switch (ContainerObject.AccessLayer.DbAccessType)
			{
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
				case DbAccessType.SqLite:
					targetAlias.Value = queryIdentifier.Value;
				break;
			}
			var updatePart = new UpdateTableWithQueryPart(queryIdentifier,
				UpdateTableWithQueryPart.ColumsOfType(dbClassInfoCache, targetAlias, queryIdentifier, ContainerObject),
				targetAlias);

			var identityInsert = DbIdentityInsertScope.Current != null;
			var include =
				dbClassInfoCache
					.Propertys
					.Select(f => f.Value)
					.Where(s =>
					{
						if (s.InsertIgnore) return false;
						if (identityInsert && s.PrimaryKeyAttribute != null) return true;
						if (s.PrimaryKeyAttribute != null) return false;
						if (s.ForginKeyAttribute != null) return false;
						return !s.UpdateIgnore;
					});

			foreach (var dbPropertyInfoCach in include)
			{
				var paramName = $"@setArg{ContainerObject.GetNextParameterId()}";
				updatePart.ColumnAssignments.Add(new UpdateTableWithQueryPart.ColumnAssignment()
				{
					Column = dbPropertyInfoCach.DbName,
					Value = paramName,
					QueryParameters =
					{
						new QueryParameter(paramName, dbPropertyInfoCach.Getter.Invoke(obj),
							dbPropertyInfoCach.PropertyType)
					}
				});
			}

			return new ElementProducer<TEntity>(Add(updatePart))
				.Where
				.PrimaryKey()
				.Is
				.EqualsTo(dbClassInfoCache.PrimaryKeyProperty.Getter.Invoke(obj));
		}

		/// <summary>
		///     Creates a Update statement for a given type
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public UpdateColumnSetters<TPoco> Table<TPoco>()
		{
			ContainerObject.Interceptors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Update, ContainerObject.AccessLayer));
			var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var targetAlias = ContainerObject.CreateTableAlias(dbClassInfoCache.TableName);
			var queryIdentifier = new QueryIdentifier()
			{
				Value = dbClassInfoCache.TableName,
				QueryIdType = QueryIdentifier.QueryIdTypes.Table
			};

			switch (ContainerObject.AccessLayer.DbAccessType)
			{
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
				case DbAccessType.SqLite:
					targetAlias.Value = queryIdentifier.Value;
					break;
			}
			return new UpdateColumnSetters<TPoco>(
				Add(new UpdateTableWithQueryPart(
					queryIdentifier,
					UpdateTableWithQueryPart.ColumsOfType(dbClassInfoCache, targetAlias, queryIdentifier, ContainerObject),
					targetAlias)));
		}
	}
}