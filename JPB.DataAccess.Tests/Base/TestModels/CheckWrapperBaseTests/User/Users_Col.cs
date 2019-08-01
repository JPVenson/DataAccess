#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[ForModel(UsersMeta.TableName)]
	public class Users_Col : INotifyPropertyChanged
	{
		private long _userId;
		private string _userName;

		[PrimaryKey]
		public long User_ID
		{
			get { return _userId; }
			set
			{
				if (value == _userId)
				{
					return;
				}
				_userId = value;
				OnPropertyChanged();
			}
		}

		public string UserName
		{
			get { return _userName; }
			set
			{
				if (value == _userName)
				{
					return;
				}
				_userName = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}