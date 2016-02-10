using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.QueryFactory;

namespace UnitTestProject1
{
	public class UsersMeta
	{
		public const string UserTable = "Users";
		public const string SelectStatement = "SELECT * FROM " + UserTable;
		public const string UserIDCol = "User_ID";
		public const string UserNameCol = "UserName";
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_Col : INotifyPropertyChanged
	{
		private long _userId;
		private string _userName;
		public event PropertyChangedEventHandler PropertyChanged;

		[JPB.DataAccess.UnitTests.Annotations.NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		[PrimaryKey]
		public long User_ID
		{
			get { return _userId; }
			set
			{
				if (value == _userId) return;
				_userId = value;
				OnPropertyChanged();
			}
		}

		public string UserName
		{
			get { return _userName; }
			set
			{
				if (value == _userName) return;
				_userName = value;
				OnPropertyChanged();
			}
		}
	}

	[AutoGenerateCtor]
	public class Users
	{
		[PrimaryKey]
		public long User_ID { get; set; }
		public string UserName { get; set; }
	}

	[AutoGenerateCtor]
	[ForModel(UsersMeta.UserTable)]
	public sealed class UsersAutoGenerateConstructor
	{
		[PrimaryKey]
		public long User_ID { get; set; }
		public string UserName { get; set; }
	}

	public class ConfigLessUser
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }
	}

	[AutoGenerateCtor]
	public class ConfigLessUserInplaceConfig
	{
		[ConfigMehtod]
		public static void Config(DbConfig configBase)
		{
			configBase.SetConfig<ConfigLessUserInplaceConfig>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.UserTable));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.UserIDCol);
				f.SetForModelKey(e => e.PropertyB, UsersMeta.UserNameCol);
			});
		}

		public long PropertyA { get; set; }
		public string PropertyB { get; set; }
	}

	public class UsersWithStaticInsert
	{
		public long User_ID { get; set; }
		public string UserName { get; set; }

		[InsertFactoryMethod]
		public string Insert()
		{
			return string.Format("INSERT INTO {0} VALUES ('{1}')", UsersMeta.UserTable, this.UserName);
		}
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_PK
	{
		[PrimaryKey]
		public long User_ID { get; set; }
		public string UserName { get; set; }
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_PK_UFM
	{
		[PrimaryKey]
		public long User_ID { get; set; }
		[ForModel(UsersMeta.UserNameCol)]
		public string UserName { get; set; }
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_PK_IDFM
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_PK_IDFM_CTORSEL
	{
		public Users_PK_IDFM_CTORSEL(IDataRecord rec)
		{
			UserName = (string)rec[UsersMeta.UserNameCol];
			UserId = (long)rec[UsersMeta.UserIDCol];
		}

		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }
	}

	[ForModel(UsersMeta.UserTable)]
	[SelectFactory(UsersMeta.SelectStatement)]
	public class Users_PK_IDFM_CLASSEL
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }
	}

	[ForModel(UsersMeta.UserTable)]
	[SelectFactory(UsersMeta.SelectStatement)]
	public class Users_PK_IDFM_FUNCSELECT
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static string GetSelectStatement()
		{
			return UsersMeta.SelectStatement;
		}
	}

	[ForModel(UsersMeta.UserTable)]
	[SelectFactory(UsersMeta.SelectStatement)]
	public class Users_PK_IDFM_FUNCSELECTFAC
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult GetSelectStatement()
		{
			return new QueryFactoryResult(UsersMeta.SelectStatement);
		}
	}

	[ForModel(UsersMeta.UserTable)]
	[SelectFactory(UsersMeta.SelectStatement)]
	public class Users_PK_IDFM_FUNCSELECTFACWITHPARAM
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult GetSelectStatement(int whereID)
		{
			return new QueryFactoryResult(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new QueryParameter("paramA", whereID));
		}
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_StaticQueryFactoryForSelect
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static void GetSelectStatement(QueryBuilder builder)
		{
			builder.Select(typeof(Users_StaticQueryFactoryForSelect));
		}
	}

	[ForModel(UsersMeta.UserTable)]
	public class Users_StaticQueryFactoryForSelectWithArugments
	{
		[PrimaryKey]
		[ForModel(UsersMeta.UserIDCol)]
		public long UserId { get; set; }
		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static void GetSelectStatement(QueryBuilder builder, long whereId)
		{
			builder.Select(typeof(Users_StaticQueryFactoryForSelect))
				.Where(UsersMeta.UserIDCol + " = @whereId", new
				{
					whereId
				});
		}
	}
}