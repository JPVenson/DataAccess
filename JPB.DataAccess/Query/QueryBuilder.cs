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
	public class QueryBuilderX : IQueryBuilder
	{
		internal QueryBuilderX(DbAccessLayer database, Type type)
		{
			this.ContainerObject = new InternalContainerContainer(database, type);
		}

		internal QueryBuilderX(IQueryContainer database)
		{
			this.ContainerObject = database;
		}

		internal QueryBuilderX(IQueryBuilder database)
		{
			this.ContainerObject = database.ContainerObject;
		}

		internal QueryBuilderX(IQueryBuilder database, Type type)
		{
			this.ContainerObject = database.ContainerObject;
			this.ContainerObject.ForType = type;
		}

		/// <summary>
		/// Creates a new Query
		/// </summary>
		/// <param name="database"></param>
		public QueryBuilderX(DbAccessLayer database)
		{
			this.ContainerObject = new InternalContainerContainer(database);
		}


		/// <summary>
		/// Wraps the current QueryBuilder into a new Form by setting the Current query type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IQueryBuilder<T> ChangeType<T>() where T : IQueryElement
		{
			return new QueryBuilder<T>(ContainerObject);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public QueryBuilderX Clone()
		{
			return new QueryBuilderX(this.ContainerObject.Clone());
		}

		public QueryBuilderX Append(QueryBuilderX right)
		{
			foreach (var part in right.ContainerObject.Parts)
			{
				this.Add(part);
			}
			return this;
		}

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


	/// <summary>
	///     Provides functions that can build SQL Querys
	/// </summary>
	public class QueryBuilder<Stack> : IQueryBuilder<Stack>
		where Stack : IQueryElement
	{
		internal QueryBuilder(DbAccessLayer database, Type type)
		{
			this.ContainerObject = new InternalContainerContainer(database, type);
		}

		internal QueryBuilder(IQueryContainer database)
		{
			this.ContainerObject = database;
		}

		internal QueryBuilder(IQueryBuilder<Stack> database)
		{
			this.ContainerObject = database.ContainerObject;
		}

		internal QueryBuilder(IQueryBuilder<Stack> database, Type type)
		{
			this.ContainerObject = database.ContainerObject;
			this.ContainerObject.ForType = type;
		}

		/// <summary>
		/// Creates a new Query
		/// </summary>
		/// <param name="database"></param>
		public QueryBuilder(DbAccessLayer database)
		{
			this.ContainerObject = new InternalContainerContainer(database);
		}
		

		/// <summary>
		/// Wraps the current QueryBuilder into a new Form by setting the Current query type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IQueryBuilder<T> ChangeType<T>() where T : IQueryElement
		{
			return new QueryBuilder<T>(ContainerObject);
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new QueryBuilder<Stack>(this.ContainerObject);
		}
		
		public IQueryContainer ContainerObject { get; private set; }

		/// <summary>
		///     Executes the Current QueryBuilder by setting the type
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		public IEnumerable<E> ForResult<E>()
		{
			return new QueryEnumerator<Stack, E>(new QueryBuilder<E, Stack>(this));
		}

		/// <summary>
		///		Executes the current QueryBuilder
		/// </summary>
		/// <returns></returns>
		public IEnumerable ForResult()
		{
			return new QueryEnumerator<Stack>(this);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (ContainerObject.ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator(ContainerObject, ContainerObject.ForType);
			return new QueryLazyEnumerator(ContainerObject, ContainerObject.ForType);
		}
	}

	/// <summary>
	/// </summary>
	public class QueryBuilder<T, Stack> : QueryBuilder<Stack>
		where Stack : IQueryElement
	{
		/// <summary>
		///     Creates a new Instance of an QueryCommand Builder that creates Database aware querys
		/// </summary>
		public QueryBuilder(DbAccessLayer database)
			: base(database, typeof(T))
		{
		}

		internal QueryBuilder(IQueryBuilder<Stack> source)
			: base(source, typeof(T))
		{

		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public new object Clone()
		{
			return new QueryBuilder<T, Stack>(this);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public new IEnumerator<T> GetEnumerator()
		{
			if (ContainerObject.ForType == null)
				throw new ArgumentNullException("No type Supplied", new Exception());

			if (ContainerObject.EnumerationMode == EnumerationMode.FullOnLoad)
				return new QueryEagerEnumerator<T>(ContainerObject, ContainerObject.ForType);
			return new QueryLazyEnumerator<T>(ContainerObject, ContainerObject.ForType);
		}
	}
}