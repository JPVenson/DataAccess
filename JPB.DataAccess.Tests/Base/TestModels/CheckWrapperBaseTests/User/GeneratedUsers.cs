#region

using System;
using System.CodeDom.Compiler;
using System.Data;
using JPB.DataAccess.Framework.DbInfoConfig;
using JPB.DataAccess.Framework.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[GeneratedCode("JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", "1.0.0.8")]
	public partial class GeneratedUsers
	{
		public long UserID { get; set; }

		public string UserName { get; set; }

		static partial void BeforeConfig();
		static partial void AfterConfig();
		static partial void BeforeConfig(ConfigurationResolver<GeneratedUsers> config);
		static partial void AfterConfig(ConfigurationResolver<GeneratedUsers> config);

		[ObjectFactoryMethod]
		public static GeneratedUsers Factory(IDataRecord record)
		{
			GeneratedUsers super;
			super = new GeneratedUsers();
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
		public static void ConfigGeneratedUsers(ConfigurationResolver<GeneratedUsers> config)
		{
			BeforeConfig();
			BeforeConfig(config);
			config.SetClassAttribute(new ForModelAttribute("Users"));
			config.SetFactory(Factory, true);
			config.SetPropertyAttribute(s => s.UserID, new ForModelAttribute("User_ID"));
			config.SetPropertyAttribute(s => s.UserID, new PrimaryKeyAttribute());
			AfterConfig();
			AfterConfig(config);
		}
	}
}