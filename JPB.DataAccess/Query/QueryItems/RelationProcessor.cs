using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig.DbInfo;
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

		public override EagarDataRecord[] Transform(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
		{
			return JoinTables(readers, entityType, context);
		}

		public EagarDataRecord[] JoinTables(EagarDataRecord[] readers, Type entityType, QueryProcessingRecordsContext context)
		{
			if (_joinTableQueryPart.TargetTableType != entityType)
			{
				return readers;
			}
			
			var restRecords = MapJoinedTable(_joinTableQueryPart,
				readers,
				entityType,
				context,
				out var targetColumn,
				out var sourceColumn,
				out var outerMostCreatedForginKeys);

			var targetTable =
				context.QueryContainer
					.AccessLayer
					.Config
					.GetOrCreateClassInfoCache(_joinTableQueryPart.TargetTableType);

			var primaryKeyColumn = context.Columns.FirstOrDefault(e =>
				e.IsEquivalentTo(targetTable.PrimaryKeyProperty.DbName.TrimAlias()) &&
				e.Alias == _joinTableQueryPart.SourceTable);

			var primaryKeyOrdinal = readers
				.FirstOrDefault()
				.GetOrdinal(primaryKeyColumn.ColumnIdentifier().TrimAlias());
			var targetColumnOrdinal = readers
				.FirstOrDefault()
				.GetOrdinal(targetColumn.ColumnIdentifier().TrimAlias());

			var fieldsOfChild = context.Columns.Select(f => f.NaturalName.TrimAlias()).ToArray();

			var reducedRecords = readers
				.GroupBy(e => e[primaryKeyOrdinal])
				.Select(e => e.First())
				.Select(record =>
				{
					var naturalReader = new EagarDataRecord(fieldsOfChild,
						new ArrayList(context
							.Columns
							.Select(f => record[f.ColumnIdentifier().TrimAlias()])
							.ToArray()));
					SetRelationOnRecord(outerMostCreatedForginKeys,
						restRecords,
						sourceColumn,
						naturalReader,
						targetColumnOrdinal
						);
					return naturalReader;
				})
				.ToArray();

			context.Columns = context.Columns.Concat(new[]
			{
				new ColumnInfo(outerMostCreatedForginKeys, null, null),
			}).ToArray(); 
			
			return reducedRecords;
		}

		private static EagarDataRecord[] MapJoinedTable(
			JoinTableQueryPart joinTableQueryPart,
			EagarDataRecord[] readers,
			Type entityType,
			QueryProcessingRecordsContext context,
			out ColumnInfo targetColumn,
			out ColumnInfo sourceColumn,
			out string forginKey)
		{
			var parentedReaders = new List<EagarDataRecord>();

			var classInfo = context.QueryContainer.AccessLayer.Config.GetOrCreateClassInfoCache(entityType);

			DbPropertyInfoCache property;

			property = classInfo.Propertys.FirstOrDefault(f =>
					f.Value.ForginKeyAttribute?.Attribute.ForeignKey == joinTableQueryPart.SourceColumn.TrimAlias())
				.Value;

			sourceColumn = context.Columns.FirstOrDefault(e =>
				e.NaturalName.TrimAlias().Equals(joinTableQueryPart.TargetColumn.TrimAlias()) &&
				e.Alias.Equals(joinTableQueryPart.Alias));

			targetColumn = context.Columns.FirstOrDefault(e =>
				e.NaturalName.TrimAlias().Equals(joinTableQueryPart.SourceColumn.TrimAlias()) &&
				e.Alias.Equals(joinTableQueryPart.SourceTable));

			if (sourceColumn == null)
			{
				throw new InvalidOperationException();
			}
			if (targetColumn == null)
			{
				throw new InvalidOperationException();
			}

			var joinedTables = new Dictionary<string, EagarDataRecord[]>();
			foreach (var subJoinTableQueryPart in joinTableQueryPart.DependingJoins)
			{
				var relMapping = MapJoinedTable(subJoinTableQueryPart,
					readers,
					subJoinTableQueryPart.TargetTableType,
					context,
					out _,
					out _,
					out var mappedTo);
				joinedTables.Add(mappedTo, relMapping.ToArray());
			}
			forginKey = property.PropertyName;
			var fields = joinTableQueryPart.Columns.ToArray();
			var fieldsOfChild = fields
				.Select((item, index) => item.ColumnIdentifier().TrimAlias())
				.ToArray();
			//var naturalFieldNames = fields.Select(f => f.NaturalName.TrimAlias()).ToArray();

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
						.FirstOrDefault(e => e.Key.ColumnIdentifier().TrimAlias().Equals(child));
				columnMapping.Add(indexOfSource.Key.NaturalName.TrimAlias(), indexOfSource.Value);
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
				if (joinedTables.Any())
				{
					var ordinal = naturalReader.GetOrdinal(sourceColumn.ColumnName.TrimAlias());
					foreach (var eagarDataRecordse in joinedTables)
					{
						SetRelationOnRecord(eagarDataRecordse.Key, 
							eagarDataRecordse.Value, 
							targetColumn, 
							naturalReader, 
							ordinal);
					}
				}
				parentedReaders.Add(naturalReader);
			}

			context.Columns = context.Columns
				.Except(joinTableQueryPart.Columns)
				.ToArray();

			return parentedReaders.ToArray();
		}

		private static void SetRelationOnRecord(string virtualColumnName,
			EagarDataRecord[] relationRecordSource,
			ColumnInfo targetColumn,
			EagarDataRecord naturalReader, 
			int ordinal)
		{
			var joinedIndexOfSearch = relationRecordSource.FirstOrDefault()
				.GetOrdinal(targetColumn.ColumnName.TrimAlias());

			naturalReader.Add(virtualColumnName, relationRecordSource.Where(join =>
			{
				var left = naturalReader[ordinal];
				var right = @join[joinedIndexOfSearch];
				return left == right || (left != null && right != null && left.Equals(right));
			}).ToArray());
		}
	}
}