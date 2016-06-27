using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Selection;

namespace JPB.DataAccess.Query.Operators
{
	public class SelectQuery<TPoco> : ElementProducer<TPoco>, ISelectQuery<TPoco>
	{
		public SelectQuery(IQueryBuilder database) : base(database)
		{
			if (CurrentIdentifier != null)
				this.QueryText("AS " + CurrentIdentifier);
		}

		public SelectQuery(IQueryBuilder database, string ident) : base(database, ident)
		{
			if (CurrentIdentifier != null)
				this.QueryText("AS " + CurrentIdentifier);
		}

		/// <summary>
		/// Retuns a collection of all Entites that are referenced by element 
		/// Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <param name="element"></param>
		/// <returns></returns>
		public ElementProducer<TPoco> In<TEPoco>(TEPoco element)
		{
			var teCache = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var pkValue = teCache.PrimaryKeyProperty.Getter.Invoke(element);
			return In<TEPoco>(pkValue);
		}


		/// <summary>
		/// Retuns a collection of all Entites that are referenced by element 
		/// Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <returns></returns>
		public ElementProducer<TPoco> In<TEPoco>(object id)
		{
			var teCache = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var fkPropertie = Cache.Propertys
				.SingleOrDefault(s =>
					s.Value.ForginKeyDeclarationAttribute != null &&
					(s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TEPoco) ||
					s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == teCache.TableName))
					.Value;

			if (fkPropertie == null)
				throw new NotSupportedException(string.Format("No matching Column was found for Forgin key declaration for table {0}", teCache.TableName));

			return new ElementProducer<TPoco>(this.Where().Column(fkPropertie.DbName).Is().EqualsTo(id));
		}
	}
}
