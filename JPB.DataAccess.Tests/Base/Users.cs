#region

using System;
using System.CodeDom.Compiler;
using System.Data;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base
{
	// Created by BORGCUBE\User
	// Created on 2016 April 30
	[GeneratedCode("JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", "1.0.0.8")]
	public partial class Users
	{
		public long UserID { get; set; }

		public string UserName { get; set; }

		static partial void BeforeConfig();
		static partial void AfterConfig();
		static partial void BeforeConfig(ConfigurationResolver<Users> config);
		static partial void AfterConfig(ConfigurationResolver<Users> config);

		[ObjectFactoryMethod]
		public static Users Factory(IDataRecord record)
		{
			Users super;
			super = new Users();
			super.UserID = (long) record["User_ID"];
			object username;
			username = record["UserName"];
			if (username == DBNull.Value)
			{
				super.UserName = null;
			}
			else
			{
				super.UserName = (string) username;
			}
			return super;
		}

		[ConfigMehtod]
		public static void ConfigUsers(ConfigurationResolver<Users> config)
		{
			BeforeConfig();
			BeforeConfig(config);
			config.SetFactory(Factory, true);
			config.SetPropertyAttribute(s => s.UserID, new ForModelAttribute("User_ID"));
			config.SetPropertyAttribute(s => s.UserID, new PrimaryKeyAttribute());
			AfterConfig();
			AfterConfig(config);
		}
	}
}