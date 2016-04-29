using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.View
{
	/// <summary>
	/// Interaction logic for ClassPreviewWindow.xaml
	/// </summary>
	public partial class ClassPreviewWindow : Window
	{
		public ClassPreviewWindow()
		{
			InitializeComponent();
		}
	}

	public class BindableTextEditor : TextEditor, INotifyPropertyChanged
	{
		/// <summary>
		/// A bindable Text property
		/// </summary>
		public new string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		/// <summary>
		/// The bindable text property dependency property
		/// </summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(BindableTextEditor), new PropertyMetadata((obj, args) =>
			{
				var target = (BindableTextEditor)obj;
				target.Text = (string)args.NewValue;
			}));

		protected override void OnTextChanged(EventArgs e)
		{
			RaisePropertyChanged("Text");
			base.OnTextChanged(e);
		}

		/// <summary>
		/// Raises a property changed event
		/// </summary>
		/// <param name="property">The name of the property that updates</param>
		public void RaisePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
