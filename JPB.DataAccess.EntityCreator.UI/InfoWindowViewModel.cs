using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI
{
	public class InfoWindowViewModel : AsyncViewModelBase
	{
		public InfoWindowViewModel()
		{
			CoreVersion = typeof(SharedMethods).Assembly.GetName().Version.ToString();
			UiVersion = typeof(InfoWindowViewModel).Assembly.GetName().Version.ToString();
			DataAccessVersion = typeof(DataConverterExtensions).Assembly.GetName().Version.ToString();
		}

		public string CoreVersion { get; set; }
		public string UiVersion { get; set; }
		public string DataAccessVersion { get; set; }
	}
}
