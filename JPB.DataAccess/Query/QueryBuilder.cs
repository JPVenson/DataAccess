/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryBuilder" />
	public class QueryBuilderX : IQueryBuilder
	{
		internal QueryBuilderX(DbAccessLayer database, Type type)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");
			if (database == null)
				throw new ArgumentNullException("type", "Please use a valid Type");

			this.ContainerObject = new InternalContainerContainer(database, type);
		}

		internal QueryBuilderX(IQueryContainer database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");

			this.ContainerObject = database;
		}

		internal QueryBuilderX(IQueryBuilder database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");

			this.ContainerObject = database.ContainerObject;
		}

		internal QueryBuilderX(IQueryBuilder database, Type type)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid Database");
			if (database == null)
				throw new ArgumentNullException("type", "Please use a valid Type");

			this.ContainerObject = database.ContainerObject;
			this.ContainerObject.ForType = type;
		}

		/// <summary>
		/// Creates a new Query
		/// </summary>
		/// <param name="database"></param>
		public QueryBuilderX(DbAccessLayer database)
		{
			if (database == null)
				throw new ArgumentNullException("database", "Please use a valid DbAccess Layer");

			this.ContainerObject = new InternalContainerContainer(database);
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public QueryBuilderX Clone()
		{
			return new QueryBuilderX(this.ContainerObject.Clone());
		}

		/// <summary>
		/// Appends the specified query Builder.
		/// </summary>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public QueryBuilderX Append(QueryBuilderX right)
		{
		    if (right.ContainerObject == this.ContainerObject)
		        return this;

			foreach (var part in right.ContainerObject.Parts)
			{
				this.Add(part);
			}
			return this;
		}

		/// <summary>
		/// The interal value holder
		/// </summary>
		public IQueryContainer ContainerObject { get; private set; }

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IEnumerable<E> ForResult<E>()
		{
			return new QueryEnumeratorEx<E>(this);
		}

		///// <summary>
		///// </summary>
		///// <returns></returns>
		//public IEnumerator GetEnumerator()
		//{
		//	if (ContainerObject.ForType == null)
		//		throw new ArgumentNullException("No type Supplied", new Exception());

		//	if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
		//		return new QueryEagerEnumerator(ContainerObject, ContainerObject.ForType);
		//	return new QueryLazyEnumerator(ContainerObject, ContainerObject.ForType);
		//}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<TPoco> GetEnumerator<TPoco>()
		{
			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator<TPoco>(ContainerObject);
			return new QueryLazyEnumerator<TPoco>(ContainerObject);
		}
	}
}