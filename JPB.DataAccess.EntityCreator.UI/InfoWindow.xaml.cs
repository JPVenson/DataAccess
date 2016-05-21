using System.Diagnostics;
using System.Windows.Navigation;

namespace JPB.DataAccess.EntityCreator.UI
{
	/// <summary>
	/// Interaction logic for InfoWindow.xaml
	/// </summary>
	public partial class InfoWindow
	{
		public InfoWindow()
		{
			this.DataContext = new InfoWindowViewModel();
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
