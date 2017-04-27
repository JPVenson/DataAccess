#region

using System;
using System.Linq;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

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
			if (CurrentIdentifier != null)
				this.QueryText("AS " + CurrentIdentifier);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SelectQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="ident">The ident.</param>
		public SelectQuery(IQueryBuilder database, string ident) : base(database, ident)
		{
			if (CurrentIdentifier != null)
				this.QueryText("AS " + CurrentIdentifier);
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
				throw new NotSupportedException(
					string.Format("No matching Column was found for Forgin key declaration for table {0}", teCache.TableName));

			return Where.Column(fkPropertie.DbName).Is.EqualsTo(id);
		}
	}
}