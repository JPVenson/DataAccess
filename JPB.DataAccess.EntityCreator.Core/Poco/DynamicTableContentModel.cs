using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.QueryBuilder;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public class DynamicTableContentModel
	{
		[ObjectFactoryMethod]
		public DynamicTableContentModel(IDataRecord record)
		{
			DataHolder = new Dictionary<string, object>();

			for (int i = 0; i < record.FieldCount; i++)
			{
				DataHolder.Add(record.GetName(i), record.GetValue(i));
			}
		}

		[SelectFactoryMethod]
		public static void SelectFromTable(IQueryBuilder<IRootQuery> queryBuilder, string tableName)
		{
			queryBuilder.SelectStar().QueryD(tableName);
		}

		[JPB.DataAccess.ModelsAnotations.LoadNotImplimentedDynamic]
		public IDictionary<string, object> DataHolder { get; set; }
	}
}