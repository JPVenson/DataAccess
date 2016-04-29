using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Highlighting;
using JPB.DataAccess.EntityCreator.Core;
using JPB.ErrorValidation;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class ClassPreviewViewModel : DataErrorBase<ClassPreviewViewModel, ClassPreviewViewModelErrorProvider>
	{
		private readonly SqlEntityCreatorViewModel _compilerInfos;

		public ClassPreviewViewModel(TableInfoViewModel sourceElement, SqlEntityCreatorViewModel compilerInfos)
		{
			_compilerInfos = compilerInfos;
			SourceElement = sourceElement;

			this.HightlightProvider = HighlightingManager.Instance.GetDefinition("C#");

			Refresh();
		}

		public IHighlightingDefinition HightlightProvider { get; set; }

		private void Refresh()
		{
			if(!CheckCanExecuteCondition())
				return;

			base.SimpleWorkWithSyncContinue(() =>
			{
				try
				{
					using (var memsStream = new MemoryStream())
					{
						SharedMethods.CompileTable(this.SourceElement, _compilerInfos, memsStream);
						memsStream.Seek(0, SeekOrigin.Begin);
						return Encoding.ASCII.GetString(memsStream.ToArray());
					}
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}, s =>
			{
				this.Result = s;
			});
		}

		private bool _keepUpdated;

		public bool KeepUpdated
		{
			get { return _keepUpdated; }
			set
			{
				SendPropertyChanging(() => KeepUpdated);
				_keepUpdated = value;
				SendPropertyChanged(() => KeepUpdated);

				if (value)
				{
					foreach (var columnInfoViewModel in this.SourceElement.ColumnInfoModels)
					{
						columnInfoViewModel.PropertyChanged += _compilerInfos_PropertyChanged;
					}
					this.SourceElement.PropertyChanged += _compilerInfos_PropertyChanged;
					this._compilerInfos.PropertyChanged += _compilerInfos_PropertyChanged;
				}
				else
				{
					foreach (var columnInfoViewModel in this.SourceElement.ColumnInfoModels)
					{
						columnInfoViewModel.PropertyChanged -= _compilerInfos_PropertyChanged;
					}
					this.SourceElement.PropertyChanged -= _compilerInfos_PropertyChanged;
					this._compilerInfos.PropertyChanged -= _compilerInfos_PropertyChanged;
				}
			}
		}

		private void _compilerInfos_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Refresh();
		}

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

		private TableInfoViewModel _sourceElement;

		public TableInfoViewModel SourceElement
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
