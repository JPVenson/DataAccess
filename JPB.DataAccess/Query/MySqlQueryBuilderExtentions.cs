//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using JPB.DataAccess.Query.Operators;

//namespace JPB.DataAccess.Query
//{
//	/// <summary>
//	///		Defines a set of extentions exclusiv valid for MySql
//	/// </summary>
//	public static class MySqlQueryBuilderExtentions
//	{

//		/// <summary>
//		///     Creates an closed sub select
//		/// </summary>
//		/// <typeparam name="T"></typeparam>
//		/// <param name="query">The query.</param>
//		/// <param name="subSelect">The sub select.</param>
//		/// <param name="identifyer">The Identifyer for this SubQuery</param>
//		/// <returns></returns>
//		public static ElementProducer<T> SubSelect<T>(this RootQuery query,
//			Func<ElementProducer<T>> subSelect, string identifyer)
//		{
//			return new ElementProducer<T>(new ElementProducer<T>(query.QueryD("SELECT * FROM ").InBracket(f => f.Append(subSelect()))).As(identifyer));
//		}
//	}
//}
