using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models
{
	public class ColumnComparer
	{
		public ColumnComparer()
		{
			ColumnInfoModel = typeof(IColumInfoModel).GetClassInfo();
		}
		
		public DbClassInfoCache ColumnInfoModel { get; private set; }

		public ColumnMergeItem Result { get; set; }

		public void Compare(IColumInfoModel left, IColumInfoModel right)
		{
			Result = new ColumnMergeItem(left, right, MergeStatus.Same);
			var mergeItems = new List<PropertyMergeItem>();
			foreach (var source in ColumnInfoModel.Propertys.Where(f => !typeof(IEnumerable).IsAssignableFrom(f.Value.PropertyType)))
			{
				var leftValue = source.Value.Getter.Invoke(left);
				var rightValue = source.Value.Getter.Invoke(right);

				if (leftValue != rightValue)
				{
					mergeItems.Add(new PropertyMergeItem(source.Key, leftValue, rightValue, MergeStatus.NotSame));
				}
			}

			if (mergeItems.Any())
			{
				Result = new ColumnMergeItem(left,right, MergeStatus.NotSame);
				Result.ColumnMerges.AddRange(mergeItems);
			}
		}
	}
}
