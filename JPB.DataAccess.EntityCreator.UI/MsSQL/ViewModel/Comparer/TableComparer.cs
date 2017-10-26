using System.Collections;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer
{
	public class TableComparer
	{
		public TableComparer()
		{
			TableInfoModel = typeof(ITableInfoModel).GetClassInfo();
		}

		public DbClassInfoCache TableInfoModel { get; private set; }

		public TableMergeItem Result { get; set; }

		public void Compare(ITableInfoModel left, ITableInfoModel right)
		{
			Result = new TableMergeItem(left, right);
			foreach (var source in TableInfoModel.Propertys.Where(f => !typeof(IEnumerable).IsAssignableFrom(f.Value.PropertyType)))
			{
				var leftValue = source.Value.Getter.Invoke(left);
				var rightValue = source.Value.Getter.Invoke(right);

				if (leftValue != rightValue)
				{
					Result.TableMerges.Add(new PropertyMergeItem(source.Key, leftValue, rightValue, MergeStatus.NotSame));
				}
			}

			foreach (var leftColumn in left.ColumnInfos)
			{
				var rightColumn = right.ColumnInfos
					.FirstOrDefault(s => s.ColumnInfo.ColumnName == leftColumn.ColumnInfo.ColumnName);

				if (rightColumn == null)
				{
					Result.ColumnMergeItems.Add(new ColumnMergeItem(leftColumn, null, MergeStatus.RightMissing));
				}
				else
				{
					var columComparer = new ColumnComparer();
					columComparer.Compare(leftColumn, rightColumn);
					Result.ColumnMergeItems.Add(columComparer.Result);
				}
			}
		}
	}
}
