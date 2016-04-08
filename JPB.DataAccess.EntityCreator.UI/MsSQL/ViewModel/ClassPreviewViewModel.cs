using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Highlighting;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class ClassPreviewViewModel : AsyncViewModelBase
	{
		public ClassPreviewViewModel(ITableInfoModel sourceElement, IMsSqlCreator compilerInfos)
		{
			SourceElement = sourceElement;

			this.HightlightProvider = HighlightingManager.Instance.GetDefinition("C#");

			base.SimpleWorkWithSyncContinue(() =>
			{
				using (var memsStream = new MemoryStream())
				{
					SharedMethods.CompileTable(this.SourceElement, compilerInfos, memsStream);
					memsStream.Seek(0, SeekOrigin.Begin);
					return Encoding.ASCII.GetString(memsStream.ToArray());
				}
			}, s =>
			{
				this.Result = s;
			});
		}

		public IHighlightingDefinition HightlightProvider { get; set; }


		private string _result;

		public string Result
		{
			get { return _result; }
			set
			{
				SendPropertyChanging(() => Result);
				_result = value;
				SendPropertyChanged(() => Result);
			}
		}

		private List<string> _errors;

		public List<string> Errors
		{
			get { return _errors; }
			set
			{
				SendPropertyChanging(() => Errors);
				_errors = value;
				SendPropertyChanged(() => Errors);
			}
		}

		private ITableInfoModel _sourceElement;

		public ITableInfoModel SourceElement
		{
			get { return _sourceElement; }
			set
			{
				SendPropertyChanging(() => SourceElement);
				_sourceElement = value;
				SendPropertyChanged(() => SourceElement);
			}
		}

		private bool _isLoading;

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				SendPropertyChanging(() => IsLoading);
				_isLoading = value;
				SendPropertyChanged(() => IsLoading);
			}
		}
	}
}
