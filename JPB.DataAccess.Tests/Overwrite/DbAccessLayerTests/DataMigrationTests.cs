#region

using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using JPB.DataAccess.Tests.TestFramework;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(GlobalTestContext.MainParallelScope)]
	public class DataMigrationTests : DatabaseStandardTest
	{
		public DataMigrationTests(DbAccessType type, bool egarLoading, bool asyncExecution, bool syncronised) 
			: base(type, egarLoading, asyncExecution, syncronised){
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		[TestCase(typeof(Users))]
		[TestCase(typeof(Users_Col))]
		[TestCase(typeof(UsersWithoutProperties))]
		[TestCase(typeof(Base.TestModels.CheckWrapperBaseTests.Users))]
		[TestCase(typeof(UsersAutoGenerateConstructor))]
		[TestCase(typeof(UsersAutoGenerateNullableConstructor))]
		[TestCase(typeof(GeneratedUsers))]
		[TestCase(typeof(ConfigLessUserInplaceConfig))]
		[TestCase(typeof(ConfigLessUserInplaceDirectConfig))]
		[TestCase(typeof(Users_PK))]
		[TestCase(typeof(Users_PK_UFM))]
		[TestCase(typeof(Users_PK_IDFM))]
		[TestCase(typeof(Users_PK_IDFM_CLASSEL))]
		[TestCase(typeof(Users_PK_IDFM_FUNCSELECT))]
		[TestCase(typeof(Users_PK_IDFM_FUNCSELECTFAC))]
		[TestCase(typeof(Users_PK_IDFM_FUNCSELECTFACWITHPARAM))]
		[TestCase(typeof(Users_StaticQueryFactoryForSelect))]
		public void AddGenericTest(Type type)
		{
			var tableName = DbAccess.Config.GetOrCreateClassInfoCache(type).TableName;

			Assert.That(() => DataMigrationHelper.ClearDb(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)), Is.Zero);
			Assert.That(DataMigrationHelper.AddEntity(DbAccess, 200, type), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)),
			Is.EqualTo(200));
			Assert.That(DataMigrationHelper.AddEntity(DbAccess, 200, type), Is.Not.Empty.And.Unique);
			Assert.That(() => DbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)),
			Is.EqualTo(400));
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