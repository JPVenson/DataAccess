using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI
{
	public class MainWindowViewModel : AsyncViewModelBase
	{
		public MainWindowViewModel()
		{
			SelectedProvider = new MsSQL.ViewModel.SqlEntityCreatorViewModel();
		}

		private IEntryCreator _selectedProvider;

		public IEntryCreator SelectedProvider
		{
			get { return _selectedProvider; }
			set
			{
				SendPropertyChanging(() => SelectedProvider);
				_selectedProvider = value;
				SendPropertyChanged(() => SelectedProvider);
			}
		}
	}
}
