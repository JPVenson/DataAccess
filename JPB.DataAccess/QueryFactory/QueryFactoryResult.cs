/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
	/// <summary>
	///     Wraps a query and its Paramters into one single Object.
	///     Can be returned by an Factory on an POCO
	/// </summary>
	public class QueryFactoryResult : IQueryFactoryResult
	{
		/// <summary>
		/// </summary>
		/// <param name="builder"></param>
		public QueryFactoryResult(QueryBuilder.QueryBuilder builder)
		{
			var compileFlat = builder.CompileFlat();
			Query = compileFlat.Item1;
			Parameters = compileFlat.Item2;
		}

		/// <summary>
		/// </summary>
		/// <param name="query"></param>
		public QueryFactoryResult(string query)
		{
			Query = query;
		}

		/// <summary>
		/// </summary>
		/// <param name="query"></param>
		/// <param name="parameters"></param>
		public QueryFactoryResult(string query, params IQueryParameter[] parameters)
			: this(query)
		{
			Parameters = parameters;
		}

		/// <summary>
		///     The SQL Query
		/// </summary>
		public string Query { get; private set; }

		/// <summary>
		///     All used Parameters
		/// </summary>
		public IEnumerable<IQueryParameter> Parameters { get; private set; }
	}
}