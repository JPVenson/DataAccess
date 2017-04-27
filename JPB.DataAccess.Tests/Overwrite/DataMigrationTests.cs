#region

using System;
using System.Diagnostics;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.DbAccessLayerTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests
{
	public class DataMigrationTests : BaseTest
	{
		public DataMigrationTests(DbAccessType type) : base(type)
		{
		}

		[Test]
		[TestCase(UsersMeta.TableName, typeof(Users))]
		[TestCase(UsersMeta.TableName, typeof(Users_Col))]
		[TestCase(UsersMeta.TableName, typeof(UsersWithoutProperties))]
		[TestCase(UsersMeta.TableName, typeof(Base.TestModels.CheckWrapperBaseTests.Users))]
		[TestCase(UsersMeta.TableName, typeof(UsersAutoGenerateConstructor))]
		[TestCase(UsersMeta.TableName, typeof(UsersAutoGenerateNullableConstructor))]
		[TestCase(UsersMeta.TableName, typeof(GeneratedUsers))]
		[TestCase(UsersMeta.TableName, typeof(ConfigLessUserInplaceConfig))]
		[TestCase(UsersMeta.TableName, typeof(ConfigLessUserInplaceDirectConfig))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_UFM))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_CLASSEL))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECT))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECTFAC))]
		[TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECTFACWITHPARAM))]
		[TestCase(UsersMeta.TableName, typeof(Users_StaticQueryFactoryForSelect))]
		public void AddGenericTest(string tableName, Type type)
		{
			using (base.MakeManager(Type, tableName, type))
			{
				Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
				Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)), Is.Zero);
				Assert.That(DataMigrationHelper.AddEntity(DbAccess, 200, type), Is.Not.Empty.And.Unique);
				Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)),
							Is.EqualTo(200));
				Assert.That(DataMigrationHelper.AddEntity(DbAccess, 200, type), Is.Not.Empty.And.Unique);
				Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)),
							Is.EqualTo(400));
			}
		}

		[Test]
		public void AddUserTest()
		{
			Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)),
			            Is.Zero);
			Assert.That(DataMigrationHelper.AddUsers(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)),
			            Is.EqualTo(200));
			Assert.That(DataMigrationHelper.AddUsers(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)),
			            Is.EqualTo(400));
		}

		[Test]
		public void AddBooksTest()
		{
			Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.Zero);
			Assert.That(DataMigrationHelper.AddBooks(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.EqualTo(200));
			Assert.That(DataMigrationHelper.AddBooks(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.EqualTo(400));
		}

		[Test]
		public void AddImagesTest()
		{
			Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.Zero);
			Assert.That(DataMigrationHelper.AddImages(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.EqualTo(200));
			Assert.That(DataMigrationHelper.AddImages(200, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.EqualTo(400));
		}

		[Test]
		public void AddBooksWithImage()
		{
			Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.Zero);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.Zero);
			Assert.That(DataMigrationHelper.AddBooksWithImage(200, 5, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.EqualTo(200));
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.EqualTo(200 * 5));
			Assert.That(DataMigrationHelper.AddBooksWithImage(200, 5, DbAccess), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)),
			            Is.EqualTo(400));
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)),
			            Is.EqualTo(400 * 5));
		}
	}
}