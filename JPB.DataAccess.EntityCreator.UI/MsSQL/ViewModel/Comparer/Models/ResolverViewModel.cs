using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
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
