using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.Helper;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using ForeignKeyConstraint = Microsoft.SqlServer.Dac.Model.ForeignKeyConstraint;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public class DacpacMsSqlStructure : IMsSqlStructure
	{
		private readonly string _dacpacPath;

		public DacpacMsSqlStructure(string dacpacPath)
		{
			_dacpacPath = dacpacPath;

			DacPackage = TSqlModel.LoadFromDacpac(dacpacPath, new ModelLoadOptions
			{
				ModelStorageType = DacSchemaModelStorageType.Memory,
				ThrowOnModelErrors = false
			});
			DacpacTypesToSqlMapping = new Dictionary<SqlDataType, SqlDbType>
			{
				{SqlDataType.Unknown, SqlDbType.Udt},
				{SqlDataType.BigInt, SqlDbType.BigInt},
				{SqlDataType.Int, SqlDbType.Int},
				{SqlDataType.SmallInt, SqlDbType.SmallInt},
				{SqlDataType.TinyInt, SqlDbType.TinyInt},
				{SqlDataType.Bit, SqlDbType.Bit},
				{SqlDataType.Decimal, SqlDbType.Decimal},
				{SqlDataType.Numeric, SqlDbType.Decimal},
				{SqlDataType.Money, SqlDbType.Money},
				{SqlDataType.SmallMoney, SqlDbType.SmallMoney},
				{SqlDataType.Float, SqlDbType.Float},
				{SqlDataType.Real, SqlDbType.Real},
				{SqlDataType.DateTime, SqlDbType.DateTime},
				{SqlDataType.SmallDateTime, SqlDbType.SmallDateTime},
				{SqlDataType.Char, SqlDbType.Char},
				{SqlDataType.VarChar, SqlDbType.VarChar},
				{SqlDataType.Text, SqlDbType.Text},
				{SqlDataType.NChar, SqlDbType.NChar},
				{SqlDataType.NVarChar, SqlDbType.NVarChar},
				{SqlDataType.NText, SqlDbType.NText},
				{SqlDataType.Binary, SqlDbType.Binary},
				{SqlDataType.VarBinary, SqlDbType.VarBinary},
				{SqlDataType.Image, SqlDbType.Image},
				{SqlDataType.Cursor, SqlDbType.Udt},
				{SqlDataType.Variant, SqlDbType.Variant},
				{SqlDataType.Timestamp, SqlDbType.Timestamp},
				{SqlDataType.UniqueIdentifier, SqlDbType.UniqueIdentifier},
				{SqlDataType.Xml, SqlDbType.Xml},
				{SqlDataType.Date, SqlDbType.Date},
				{SqlDataType.Time, SqlDbType.Time},
				{SqlDataType.DateTime2, SqlDbType.DateTime2},
				{SqlDataType.DateTimeOffset, SqlDbType.DateTimeOffset},
				{SqlDataType.Rowversion, SqlDbType.Timestamp}
			};
		}

		public TSqlModel DacPackage { get; set; }

		public IDictionary<SqlDataType, SqlDbType> DacpacTypesToSqlMapping { get; set; }

		private TSqlObject GetTable(string table, string database)
		{
			var externalParts = table.Split('.').Select(f => f.Trim('[', ']'));
			var sqlObject = DacPackage.GetObject(Table.TypeClass, new ObjectIdentifier(externalParts),
				                DacQueryScopes.All) ??
			                DacPackage.GetObject(View.TypeClass, new ObjectIdentifier(externalParts),
				                DacQueryScopes.All);
			return sqlObject;
		}

		private ModelRelationshipInstance[] GetColumns(string table, string database)
		{
			var sqlObject = GetTable(table, database);
			if (sqlObject.ObjectType == Table.TypeClass)
			{
				return sqlObject.GetReferencedRelationshipInstances(Table.Columns).ToArray();
			}
			if (sqlObject.ObjectType == View.TypeClass)
			{
				return sqlObject.GetReferencedRelationshipInstances(View.Columns).ToArray();
			}
			return new ModelRelationshipInstance[0];
		}

		public ColumnInfo[] GetColumnsOf(string table, string database)
		{
			return GetColumns(table, database)
				.Select((item, index) =>
				{
					var info = new ColumnInfo();
					info.ColumnName = item.Object.Name.ToString();
					info.Nullable = item.Object.GetProperty<bool>(Column.Nullable);
					info.MaxLength = item.Object.GetProperty<int?>(Column.Length);
					info.PositionFromTop = index;
					SqlDataType sqlDataType;

					var itemDataType = item.Object.GetReferenced(Column.DataType).ToArray().FirstOrDefault();
					if (itemDataType != null)
					{
						sqlDataType = itemDataType
							.GetProperty<SqlDataType>(DataType.SqlDataType);
					}
					else
					{
						//if this is an view its a generated column and we need to obtain the source column
						var refs = item.Object.GetReferenced().FirstOrDefault(e => e.ObjectType == Column.TypeClass);
						if (refs != null)
						{
							var innerItemDataType = refs.GetReferenced(Column.DataType).ToArray().FirstOrDefault();
							if (innerItemDataType != null)
							{
								sqlDataType = innerItemDataType
									.GetProperty<SqlDataType>(DataType.SqlDataType);
							}
							else
							{
								throw new InvalidOperationException($"Could not obtain a valid column type for '{item.Object.Name}'");	
							}
							
						}
						else
						{
							var relation = item.Object.GetChildren();
							throw new InvalidOperationException($"Could not obtain a valid column type for '{item.Object.Name}'");
						}
					}


					info.SqlType = DacpacTypesToSqlMapping[sqlDataType];
					return info;
				}).ToArray();
		}

		public string GetPrimaryKeyOf(string table, string database)
		{
			return GetColumns(table, database)
				.FirstOrDefault(e => e.Object.GetProperty<bool>(Column.IsIdentity))?.Object.Name.ToString();
		}

		public ForgeinKeyInfoModel[] GetForeignKeys(string table, string database)
		{
			var tableObject = GetTable(table, database);
			var references = new List<ForgeinKeyInfoModel>();
			foreach (var sqlObject in tableObject.GetReferencing(ForeignKeyConstraint.Host))
			{
				var forginTable = sqlObject.GetReferenced(ForeignKeyConstraint.ForeignTable).FirstOrDefault().Name.ToString();
				var foreignPrimaryKey = sqlObject.GetReferenced(ForeignKeyConstraint.ForeignColumns).FirstOrDefault().Name.ToString();
				var foreignKey = sqlObject.GetReferenced(ForeignKeyConstraint.Columns).FirstOrDefault().Name.ToString();
				references.Add(new ForgeinKeyInfoModel()
				{
					TargetColumn = foreignPrimaryKey,
					SourceColumn = foreignKey,
					TableName = forginTable
				});
			}
			return references.ToArray();
		}

		public string GetVersion()
		{
			return DacPackage.Version.ToString();
		}

		public TableInformations[] GetTables()
		{
			return DacPackage.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table)
				.Select(f => new TableInformations
				{
					TableName = f.Name.ToString()
				})
				.ToArray();
		}

		public ViewInformation[] GetViews()
		{
			return DacPackage.GetObjects(DacQueryScopes.UserDefined, ModelSchema.View)
				.Select(f => new ViewInformation
				{
					TableName = f.Name.ToString()
				})
				.ToArray();
		}

		public StoredProcedureInformation[] GetStoredProcedures()
		{
			return DacPackage.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Procedure)
				.Select(f => new StoredProcedureInformation
				{
					TableName = f.Name.ToString()
				})
				.ToArray();
		}

		public Any[] GetEnumValuesOfType(string tableName)
		{
			throw new NotImplementedException();
		}

		public string GetDatabaseName()
		{
			return "?";
			var sqlObjects = DacPackage.GetObjects(DacQueryScopes.UserDefined, ModelSchema.DatabaseOptions);
			return sqlObjects.FirstOrDefault().Name.ToString();
			throw new NotImplementedException();
		}
	}
}