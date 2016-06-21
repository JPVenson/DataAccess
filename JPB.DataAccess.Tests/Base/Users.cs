using System.CodeDom.Compiler;
using System.Data;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base
{
	// Created by BORGCUBE\User
	// Created on 2016 April 30
	[GeneratedCode("JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", "1.0.0.8")]
	public sealed partial class Users
	{
		public Users()
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
		static partial void BeforeConfig(ConfigurationResolver<Users> config);
		static partial void AfterConfig(ConfigurationResolver<Users> config);
		[ObjectFactoryMethod()]
		public static Users Factory(IDataRecord record)
		{
			Users super;
			super = new Users();
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
		[ConfigMehtod()]
		public static void ConfigUsers(ConfigurationResolver<Users> config)
		{
			Users.BeforeConfig();
			Users.BeforeConfig(config);
			config.SetFactory(Users.Factory, true);
			config.SetPropertyAttribute(s => s.UserID, new ForModelAttribute("User_ID"));
			config.SetPropertyAttribute(s => s.UserID, new PrimaryKeyAttribute());
			Users.AfterConfig();
			Users.AfterConfig(config);
		}
	}
}
