using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class RelationProcessor : EntityProcessorBase
	{
		private readonly JoinParseInfo _joinTableQueryPart;

		public RelationProcessor(JoinParseInfo joinTableQueryPart)
		{
			_joinTableQueryPart = joinTableQueryPart;
			Mappings = new List<RelationMapping>();
		}

		public IList<RelationMapping> Mappings { get; private set; }

		public class RelationMapping
		{
			public ColumnInfo SourceColumn { get; set; }
			public ColumnInfo TargetColumn { get; set; }
			public IEnumerable<ColumnInfo> TargetColumns { get; set; }
			public IEnumerable<ColumnInfo> SourceColumns { get; set; }
			public EagarDataRecord[] Records { get; set; }
			public string TargetName { get; set; }
			public Type TargetType { get; set; }
			public QueryIdentifier SourceAlias { get; set; }
			public QueryIdentifier TargetAlias { get; set; }
		}

		public override EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
		{
			return JoinTables(readers, entityType, context);
		}

		public EagarDataRecord[] DoJoinMapping(
			EagarDataRecord[] readers,
			QueryIdentifier sourceAlias,
			QueryProcessingRecordsContext context,
			bool sourceJoin = true,
			bool toplevel = false)
		{
			foreach (var mapping in Mappings
				.Where(e => sourceJoin ? e.SourceAlias.Equals(sourceAlias) : e.TargetAlias.Equals(sourceAlias)).ToArray())
			{
				var targetColumnsIndexMapping = new Dictionary<ColumnInfo, int>();
				var sourceColumnsIndexMapping = new Dictionary<ColumnInfo, int>();
				var targetColumns = mapping.TargetColumns.ToArray();
				for (int index = 0; index < targetColumns.Length; index++)
				{
					var columnInfo = targetColumns[index];

					if (targetColumnsIndexMapping.ContainsKey(columnInfo))
					{
						throw new InvalidOperationException($"1Error while mapping columns. Column: '{columnInfo.NaturalName}' exists")
						{
							Data = { { "Columns", targetColumnsIndexMapping } }
						};
					}

					targetColumnsIndexMapping.Add(columnInfo, index);
				}
				
				var sourceColumns = mapping.SourceColumns.ToArray();
				for (int index = 0; index < sourceColumns.Length; index++)
				{
					var columnInfo = sourceColumns[index];

					if (sourceColumnsIndexMapping.ContainsKey(columnInfo))
					{
						throw new InvalidOperationException($"2Error while mapping columns. Column: '{columnInfo.NaturalName}' exists")
						{
							Data = { { "Columns", sourceColumnsIndexMapping } }
						};
					}

					sourceColumnsIndexMapping.Add(columnInfo, index);
				}
				var targetTable =
					context.QueryContainer
						.AccessLayer
						.Config
						.GetOrCreateClassInfoCache(mapping.TargetType);

				var primaryKeyColumn = mapping.SourceColumns.FirstOrDefault(e =>
					e.IsEquivalentTo(targetTable.PrimaryKeyProperty.DbName) &&
					e.Alias.Equals(mapping.TargetAlias));

				var primaryKeyOrdinal = sourceColumnsIndexMapping
					.FirstOrDefault(f => f.Key.IsEqualsTo(primaryKeyColumn));
				
				var targetOrdinal = sourceColumnsIndexMapping
					.FirstOrDefault(f => f.Key.IsEqualsTo(mapping.TargetColumn));

				var sourceOrdinal = targetColumnsIndexMapping
					.FirstOrDefault(f => f.Key.IsEqualsTo(mapping.SourceColumn));

				readers = readers
					.GroupBy(e => e[primaryKeyOrdinal.Value])
					.Select(e => e.First())
					.Select(record =>
					{
						SetRelationOnRecord(mapping.TargetName, 
							mapping.Records,
							targetOrdinal.Value, 
							record,
							sourceOrdinal.Value);
						return record;
					})
					.ToArray();
				Mappings.Remove(mapping);
				DoJoinMapping(mapping.Records, mapping.SourceAlias, context, false);

				if (toplevel)
				{
					context.Columns = context.Columns.Concat(new[]
					{
						new ColumnInfo(mapping.TargetName, null, null),
					}).ToArray();
				}
			}
			return readers;
		}

		public EagarDataRecord[] JoinTables(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
		{
			if (_joinTableQueryPart.TargetTableType != entityType)
			{
				return readers;
			}

			CreateJoinMapping(_joinTableQueryPart,
				readers,
				entityType,
				context);
			return readers;

			//var targetTable =
			//	context.QueryContainer
			//		.AccessLayer
			//		.Config
			//		.GetOrCreateClassInfoCache(_joinTableQueryPart.TargetTableType);

			//var primaryKeyColumn = context.Columns.FirstOrDefault(e =>
			//	e.IsEquivalentTo(targetTable.PrimaryKeyProperty.DbName) &&
			//	e.Alias == _joinTableQueryPart.SourceTable);

			//var sourceColumnsIndexMapping = new Dictionary<ColumnInfo, int>();
			//var columnInfos = context.Columns.ToArray();
			//for (int index = 0; index < columnInfos.Length; index++)
			//{
			//	var columnInfo = columnInfos[index];

			//	if (sourceColumnsIndexMapping.ContainsKey(columnInfo))
			//	{
			//		throw new InvalidOperationException($"1Error while mapping columns. Column: '{columnInfo.NaturalName}' exists")
			//		{
			//			Data = { { "Columns", sourceColumnsIndexMapping } }
			//		};
			//	}

			//	sourceColumnsIndexMapping.Add(columnInfo, index);
			//}

			//var primaryKeyCache = sourceColumnsIndexMapping
			//	.FirstOrDefault(f => f.Key.IsEqualsTo(primaryKeyColumn));

			//var targetColumnCache = sourceColumnsIndexMapping
			//	.FirstOrDefault(f => f.Key.IsEqualsTo(mapping.TargetColumn));

			//if (targetColumnCache.Key == null)
			//{
			//	throw new InvalidOperationException();
			//}

			//if (primaryKeyCache.Key == null)
			//{
			//	throw new InvalidOperationException();
			//}

			//var primaryKeyOrdinal = primaryKeyCache
			//	.Value;

			//var targetColumnOrdinal = targetColumnCache
			//		.Value;

			//var identifierNames = context.Columns.Select(e => e.ColumnIdentifier().TrimAlias())
			//	.ToArray();

			//var reducedRecords = readers
			//	.GroupBy(e => e[primaryKeyOrdinal])
			//	.Select(e => e.First())
			//	.Select(record =>
			//	{
			//		var naturalReader = new EagarDataRecord(identifierNames,
			//			new ArrayList(sourceColumnsIndexMapping
			//				.Select(f => record[f.Value])
			//				.ToArray()));
			//		if (restRecords.Any())
			//		{
			//			SetRelationOnRecord(outerMostCreatedForginKeys,
			//			   restRecords,
			//			   sourceColumn,
			//			   naturalReader,
			//			   targetColumnOrdinal
			//			   );
			//		}
			//		return naturalReader;
			//	})
			//	.ToArray();

			//context.Columns = context.Columns.Concat(new[]
			//{
			//		new ColumnInfo(outerMostCreatedForginKeys, null, null),
			//	}).ToArray();

		}

		private void CreateJoinMapping(
			JoinParseInfo joinTableQueryPart,
			EagarDataRecord[] readers,
			Type entityType,
			QueryProcessingRecordsContext context)
		{
			var parentedReaders = new List<EagarDataRecord>();
			var property = joinTableQueryPart.TargetProperty;

			var sourceColumn = joinTableQueryPart.TargetColumnName;
			var targetColumn = joinTableQueryPart.SourceColumnName;

			if (sourceColumn == null)
			{
				throw new InvalidOperationException();
			}
			if (targetColumn == null)
			{
				throw new InvalidOperationException();
			}

			var fields = joinTableQueryPart.Columns.ToArray();
			var fieldsOfChild = fields
				.Select((item, index) => item.ColumnIdentifier())
				.ToArray();

			var columnMapping = new Dictionary<string, int>();
			var sourceColumnsIndexMapping = context.Columns.ToArray().Select((item, index) => new
			{
				item,
				index
			}).ToDictionary(e => e.item, e => e.index);

			foreach (var fieldOfChild in fieldsOfChild)
			{
				var child = fieldOfChild;
				var indexOfSource =
					sourceColumnsIndexMapping
						.FirstOrDefault(e => e.Key.ColumnIdentifier().Equals(child));
				if (columnMapping.ContainsKey(indexOfSource.Key.NaturalName))
				{
					throw new InvalidOperationException($"Column name collision detected. Column '{indexOfSource.Key.NaturalName}'")
					{
						Data =
						{
							{ "Columns", sourceColumnsIndexMapping },
							{ "Field", indexOfSource },
						}
					};
				}

				columnMapping.Add(indexOfSource.Key.NaturalName, indexOfSource.Value);
			}

			var groupBy = columnMapping[columnMapping.FirstOrDefault().Key];
			//TODO might not be the PrimaryKey of the foreign table

			var readerGroups = readers
				.GroupBy(e => e[groupBy])
				.Select(e => e.First())
				.ToArray();

			foreach (var readerGroup in readerGroups)
			{
				var naturalReader = new EagarDataRecord(columnMapping.Keys.ToArray(),
					new ArrayList(columnMapping.Values.Select(f => readerGroup[f]).ToArray()));

				parentedReaders.Add(naturalReader);
			}

			Mappings.Add(new RelationMapping()
			{
				TargetColumns = joinTableQueryPart.Columns,
				SourceColumns = context.Columns
					.Where(e => e.Alias.Equals(joinTableQueryPart.SourceTable))
					.ToArray(),
				Records = parentedReaders.ToArray(),
				SourceColumn = sourceColumn,
				TargetColumn = targetColumn,
				TargetName = property.PropertyName,
				TargetType = entityType,
				SourceAlias = joinTableQueryPart.Alias,
				TargetAlias = joinTableQueryPart.SourceTable
			});

			if (readerGroups.Any())
			{
				foreach (var subJoinTableQueryPart in joinTableQueryPart.DependingJoins)
				{
					CreateJoinMapping(subJoinTableQueryPart,
						readers,
						subJoinTableQueryPart.TargetTableType,
						context);
				}
			}

			foreach (var eagarDataRecord in readers)
			{
				foreach (var columnInfo in joinTableQueryPart.Columns)
				{
					//eagarDataRecord.Remove(columnInfo.ColumnIdentifier());
					eagarDataRecord.Remove(columnInfo.ColumnIdentifier().TrimAlias());
				}
			}

			context.Columns = context.Columns
				.Except(joinTableQueryPart.Columns)
				.ToArray();
		}

		private static void SetRelationOnRecord(string virtualColumnName,
			EagarDataRecord[] relationRecordSource,
			int targetOrdinal,
			EagarDataRecord naturalReader,
			int sourceOrdinal)
		{
			naturalReader.Add(virtualColumnName, relationRecordSource.Where(join =>
			{
				var left = naturalReader[targetOrdinal];
				var right = @join[sourceOrdinal];
				if (left == null && right == null)
				{
					return false;
				}

				return left == right || (left != null && right != null && left.Equals(right));
			}).ToArray());
		}
	}
}