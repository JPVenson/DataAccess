﻿namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="IElementProducer{T}" />
	public interface ISelectQuery<out T> : IElementProducer<T>
	{
	}
}