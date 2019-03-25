#region

using System;
using System.Linq;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.ISelectQuery{TPoco}" />
	public class SelectQuery<TPoco> : ElementProducer<TPoco>, ISelectQuery<TPoco>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SelectQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public SelectQuery(IQueryBuilder database) : base(database)
		{
			CreateNewIdentifier();
		}

		/// <summary>
		///		Selects items Distinct
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> Distinct()
		{
			ContainerObject.Search<ISelectableQueryPart>().Distinct = true;
			return this;
		}

		/// <summary>
		///		Includes the forgin table
		/// </summary>
		/// <param name="forginColumnName"></param>
		/// <returns></returns>
		public SelectQuery<TPoco> Include(string forginColumnName)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var forginColumn = teCache.Propertys.FirstOrDefault(e => e.Value.PropertyName.Equals(forginColumnName));
			if (forginColumn.Value == null)
			{
				return this;
			}

			var currentAlias = ContainerObject.Search<ISelectableQueryPart>().Alias;
			var parentAlias = ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.Table);
			var childAlias = ContainerObject.GetAlias(QueryIdentifier.QueryIdTypes.Table);
			var selfPrimaryKey = teCache.PrimaryKeyProperty.DbName;
			var forginPrimaryKey = forginColumn.Value.ForginKeyAttribute.Attribute.KeyName;
			var forginType = ContainerObject.AccessLayer.GetClassInfo(forginColumn.Value.PropertyType);
			var forginColumns = DbAccessLayer.GetSelectableColumnsOf(forginType, childAlias.GetAlias());

			var joinTableQueryPart = new JoinTableQueryPart(currentAlias, 
				childAlias,
				parentAlias,
				typeof(TPoco),
				selfPrimaryKey,
				forginPrimaryKey, 
				forginColumns);
			ContainerObject.Search<SelectTableQueryPart>().AddJoin(joinTableQueryPart);
			return new SelectQuery<TPoco>(Add(joinTableQueryPart));
		}
		
		/// <summary>
		///     Retuns a collection of all Entites that are referenced by element
		///     Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <param name="element"></param>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> In<TEPoco>(TEPoco element)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var pkValue = teCache.PrimaryKeyProperty.Getter.Invoke(element);
			return In<TEPoco>(pkValue);
		}

		/// <summary>
		///     Retuns a collection of all Entites that are referenced by element
		///     Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> In<TEPoco>(object id)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var fkPropertie = Cache.Propertys
				.SingleOrDefault(s =>
					s.Value.ForginKeyDeclarationAttribute != null &&
					(s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TEPoco) ||
					 s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == teCache.TableName))
				.Value;

			if (fkPropertie == null)
			{
				throw new NotSupportedException(
				string.Format("No matching Column was found for Forgin key declaration for table {0}", teCache.TableName));
			}

			return Where.Column(fkPropertie.DbName).Is.EqualsTo(id);
		}
	}
}