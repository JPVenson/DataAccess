using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class RelationProcessor : EntityProcessorBase
	{
		private readonly JoinTableQueryPart _joinTableQueryPart;

		public RelationProcessor(JoinTableQueryPart joinTableQueryPart)
		{
			_joinTableQueryPart = joinTableQueryPart;
		}

		public override EagarDataRecord[] Transform(EagarDataRecord[] readers, 
			Type entityType,
			QueryProcessingRecordsContext context)
		{
			if (_joinTableQueryPart.Type != entityType)
			{
				return readers;
			}

			var parentedReaders = new List<EagarDataRecord>();
			var readerGroups = readers.GroupBy(e => e[_joinTableQueryPart.ParentColumn]).ToArray();
			var classInfo = context.QueryContainer.AccessLayer.Config.GetOrCreateClassInfoCache(entityType);
			var property = classInfo.Propertys.FirstOrDefault(f =>
				f.Value.ForginKeyDeclarationAttribute.Attribute?.ForeignKey == _joinTableQueryPart.ChildColumn);

			foreach (var readerGroup in readerGroups)
			{
				var fieldsOfChild = _joinTableQueryPart.Columns.Select(f => f.ColumnAliasStatement()).ToArray();
				var joinedReaders = readerGroup.Select(e => EagarDataRecord.WithIncludedFields(e,
					fieldsOfChild))
					.ToArray();
				foreach (var contextQueryContainerPostProcessor in context.QueryContainerPostProcessors)
				{
					joinedReaders =
						contextQueryContainerPostProcessor
							.Transform(joinedReaders, property.Value.PropertyType, context);
				}
				var reader = EagarDataRecord.WithExcludedFields(readerGroup.First(),
					fieldsOfChild);
				reader.Add(property.Key, joinedReaders);
				
				parentedReaders.Add(reader);
			}

			return parentedReaders.ToArray();
		}
	}
}