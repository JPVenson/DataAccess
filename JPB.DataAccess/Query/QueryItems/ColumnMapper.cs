using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems
{
	internal abstract class EntityProcessorBase : IEntityProcessor
	{
		public virtual IDbCommand BeforeExecution(IDbCommand command)
		{
			return command;
		}

		public virtual object Transform(object entity, Type entityType, QueryProcessingEntitiesContext context)
		{
			return entity;
		}

		public virtual EagarDataRecord Transform(EagarDataRecord reader, Type entityType, QueryProcessingRecordsContext context)
		{
			return reader;
		}

		public virtual EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
		{
			return readers;
		}
	}

	//internal class ColumnMapper : EntityProcessorBase
	//{
	//	public ColumnMapper()
	//	{
	//		Mappings = new Dictionary<Type, ColumnInfo[]>();
	//	}

	//	public IDictionary<Type, ColumnInfo[]> Mappings { get; private set; }

	//	public override EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
	//	{
	//		if (Mappings.TryGetValue(entityType, out var mappings))
	//		{
	//			return readers.Select(f =>
	//					new EagarDataRecord(mappings.Select(e => e.ColumnName.TrimAlias()).ToArray(), f.Objects))
	//				.ToArray();
	//		}

	//		return readers;
	//	}
	//}
}