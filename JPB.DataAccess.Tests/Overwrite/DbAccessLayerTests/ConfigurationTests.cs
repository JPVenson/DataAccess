﻿#region

using System;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Self)]
	public class ConfigurationTests : DatabaseBaseTest
	{
		public ConfigurationTests(DbAccessType type, bool asyncExecution, bool syncronised) : base(type, asyncExecution, syncronised)
		{
		}

		[Test]
		public void ConfigLess()
		{
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			DbAccess.Config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName);
				f.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName);
			});

			DbAccess.Insert(new ConfigLessUser {PropertyB = insGuid});

			var elements = DbAccess.Select<ConfigLessUser>();
			Assert.AreEqual(elements.Length, 1);

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		public void ConfigLessFail()
		{
			DbConfig.Clear();
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			DbAccess.Config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName + "TEST");
				f.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName + "TEST");
			});

			//			var unexpected = typeof(Exception);

			//#if MsSql
			//			unexpected = typeof(SqlException);
			//#endif
			//#if SqLite
			//			unexpected = typeof(SQLiteException);
			//#endif

			Assert.That(() => { DbAccess.Insert(new ConfigLessUser {PropertyB = insGuid}); }, Throws.Exception);
		}

		[Test]
		public void ConfigLessInplace()
		{
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			DbAccess.Insert(new ConfigLessUserInplaceConfig {PropertyB = insGuid});

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));
			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);

			var elements = DbAccess.Select<ConfigLessUserInplaceConfig>();
			Assert.AreEqual(elements.Length, 1);
		}
	}
}