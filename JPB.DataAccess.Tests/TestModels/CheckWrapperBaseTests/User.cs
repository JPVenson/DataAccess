using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query;
using JPB.DataAccess.QueryFactory;
using System;
using System.CodeDom.Compiler;
using JPB.DataAccess.Query.Operators;

namespace JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests
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

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
	[ForModel(UsersMeta.UserTable)]
	public class UsersWithoutProperties
	{
		[PrimaryKey]
		public long User_ID { get; set; }
	}

	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
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

	[AutoGenerateCtor]
	[ForModel(UsersMeta.UserTable)]
	public class UsersAutoGenerateNullableConstructor
	{
		[PrimaryKey]
		public Nullable<long> User_ID { get; set; }

		public string UserName { get; set; }
	}

	[AutoGenerateCtor]
	[ForModel(UsersMeta.UserTable)]
	public sealed class UsersAutoGenerateConstructorWithSingleXml
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }

		[FromXml("Sub", LoadStrategy = LoadStrategy.NotIncludeInSelect)]
		[InsertIgnore]
		public UsersAutoGenerateConstructorWithSingleXml Sub { get; set; }
	}

	[AutoGenerateCtor]
	[ForModel(UsersMeta.UserTable)]
	public sealed class UsersAutoGenerateConstructorWithMultiXml
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }

		[FromXml("Subs", LoadStrategy = LoadStrategy.NotIncludeInSelect)]
		[InsertIgnore]
		public IEnumerable<UsersAutoGenerateConstructorWithSingleXml> Subs { get; set; }
	}

	public class ConfigLessUser
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }
	}

	[GeneratedCodeAttribute("JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", "1.0.0.8")]
	public sealed partial class GeneratedUsers
	{
		public GeneratedUsers()
		{
		}
		private long _userID;
		public long UserID
		{
			get
			{
				return this._userID;
			}
			set
			{
				this._userID = value;
			}
		}
		private string _userName;
		public string UserName
		{
			get
			{
				return this._userName;
			}
			set
			{
				this._userName = value;
			}
		}
		static partial void BeforeConfig();
		static partial void AfterConfig();
		static partial void BeforeConfig(ConfigurationResolver<GeneratedUsers> config);
		static partial void AfterConfig(ConfigurationResolver<GeneratedUsers> config);
		[ObjectFactoryMethodAttribute()]
		public static GeneratedUsers Factory(IDataRecord record)
		{
			GeneratedUsers super;
			super = new GeneratedUsers();
			super.UserID = ((long)(record["User_ID"]));
			object username;
			username = record["UserName"];
			if ((username == System.DBNull.Value))
			{
				super.UserName = null;
			}
			else
			{
				super.UserName = ((string)(username));
			}
			return super;
		}
		[ConfigMehtodAttribute()]
		public static void ConfigGeneratedUsers(ConfigurationResolver<GeneratedUsers> config)
		{
			GeneratedUsers.BeforeConfig();
			GeneratedUsers.BeforeConfig(config);
			config.SetClassAttribute(new ForModelAttribute("Users"));
			config.SetFactory(GeneratedUsers.Factory, true);
			config.SetPropertyAttribute(s => s.UserID, new ForModelAttribute("User_ID"));
			config.SetPropertyAttribute(s => s.UserID, new PrimaryKeyAttribute());
			GeneratedUsers.AfterConfig();
			GeneratedUsers.AfterConfig(config);
		}
	}

	public class ConfigLessUserInplaceConfig
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }

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
	}

	public class ConfigLessUserInplaceDirectConfig
	{
		public long PropertyA { get; set; }
		public string PropertyB { get; set; }

		[ConfigMehtod]
		public static void Config(ConfigurationResolver<ConfigLessUserInplaceConfig> configBase)
		{
			configBase.SetClassAttribute(new ForModelAttribute(UsersMeta.UserTable));
			configBase.SetPrimaryKey(e => e.PropertyA);
			configBase.SetForModelKey(e => e.PropertyA, UsersMeta.UserIDCol);
			configBase.SetForModelKey(e => e.PropertyB, UsersMeta.UserNameCol);
		}
	}

	public class UsersWithStaticInsert
	{
		public long User_ID { get; set; }
		public string UserName { get; set; }

		[InsertFactoryMethod(TargetDatabase = DbAccessType.MsSql)]
		public string Insert()
		{
			return string.Format("INSERT INTO {0} VALUES ('{1}')", UsersMeta.UserTable, UserName);
		}

		[InsertFactoryMethod(TargetDatabase = DbAccessType.SqLite)]
		public string InsertSqLite()
		{
			return string.Format("INSERT INTO {0} VALUES (0, '{1}')", UsersMeta.UserTable, UserName);
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
			return new QueryFactoryResult(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA",
				new QueryParameter("paramA", whereID));
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
		public static void GetSelectStatement(RootQuery builder)
		{
			builder.Select<Users_StaticQueryFactoryForSelect>();
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
		public static void GetSelectStatement(RootQuery builder, long whereId)
		{
			builder.Select<Users_StaticQueryFactoryForSelectWithArugments>()
				.Where()
				.Column(s => s.UserId)
				.Is(whereId);
		}
	}
}