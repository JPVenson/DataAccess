using System;
using System.Collections.Generic;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public interface IDefaultConstraints<TEntity> : ICollection<ILocalDbDefaultConstraint<TEntity>>
	{
		void Add<TValue>(DbConfig config, string name, Func<TValue> generateValue, Expression<Func<TEntity, TValue>> column);
		void Add<TValue>(string name, TValue value, Action<TEntity, TValue> setter);
		void Enforce(TEntity elementToAdd);
	}
}