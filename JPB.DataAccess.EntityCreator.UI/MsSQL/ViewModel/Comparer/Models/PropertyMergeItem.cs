using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models
{
	public class PropertyMergeItem : AsyncViewModelBase
	{
		public PropertyMergeItem(string name, object valueLeft, object valueRight, MergeStatus mergeStatus)
		{
			Name = name;
			ValueLeft = valueLeft;
			ValueRight = valueRight;
			MergeStatus = mergeStatus;
		}

		public MergeStatus MergeStatus { get; private set; }

		public string Name { get; private set; }
		public object ValueLeft { get; private set; }
		public object ValueRight { get; private set; }

		private object _valueMerge;

		public object ValueMerge
		{
			get { return _valueMerge; }
			set
			{
				SendPropertyChanging(() => ValueMerge);
				_valueMerge = value;
				SendPropertyChanged(() => ValueMerge);
			}
		}

		//public string Name { get; set; }

		//public object ValueLeft { get; set; }

		//public object ValueRight { get; set; }

		//public object ValueMerge { get; set; }
	}
}