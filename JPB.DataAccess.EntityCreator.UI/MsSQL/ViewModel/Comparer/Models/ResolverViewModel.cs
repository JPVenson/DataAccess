using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.UI.Shared.Model;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel.Comparer.Models
{
	public class ResolverViewModel : AsyncViewModelBase
	{
		public ConfigStore Left { get; set; }
		public ConfigStore Right { get; set; }

		public ResolverViewModel(ConfigStore left, ConfigStore right)
		{
			Left = left;
			Right = right;
		}

		public void Resolve()
		{
			
		}
	}
}
