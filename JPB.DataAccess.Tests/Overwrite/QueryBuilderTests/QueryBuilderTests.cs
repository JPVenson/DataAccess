﻿using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.QueryBuilderTests
{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	public class QueryBuilderTests
	{
		private readonly DbAccessType _type;

		public QueryBuilderTests(DbAccessType type)
		{
			_type = type;
			Init();
		}

		private DbAccessLayer _dbAccess;
		private IManager _mgr;

		[SetUp]
		public void Init()
		{
			_mgr = new Manager();
			_dbAccess = _mgr.GetWrapper(_type);
		}

		[TearDown]
		public void TestTearDown()
		{
			// inc. class name
			var fullNameOfTheMethod = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			// method name only
			var methodName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
			// the state of the test execution
			var state = NUnit.Framework.TestContext.CurrentContext.Result.Outcome == ResultState.Failure; // TestState enum

			if (state)
			{
				_mgr.FlushErrorData();
			}
		}

		public DbAccessLayer DbAccessLayer
		{
			get { return _dbAccess; }
		}

		[Test]
		public void Select()
		{
			DataMigrationHelper.AddUsers(250, DbAccessLayer);

			int runPrimetivSelect = -1;
			if (DbAccessLayer.DbAccessType == DbAccessType.MsSql)
			{
				runPrimetivSelect = DbAccessLayer.RunPrimetivSelect<int>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))[0];
			}
			if (DbAccessLayer.DbAccessType == DbAccessType.SqLite)
			{
				runPrimetivSelect = (int)DbAccessLayer.RunPrimetivSelect<long>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))[0];
			}
			var deSelect = DbAccessLayer.Select<Users>();
			var forResult = DbAccessLayer.Query().Select<Users>().ForResult().ToArray();

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult.Count()));
			Assert.That(deSelect.Length, Is.EqualTo(forResult.Count()));

			for (int index = 0; index < forResult.Length; index++)
			{
				var userse = forResult[index];
				var userbe = deSelect[index];
				Assert.That(userse.UserID, Is.EqualTo(userbe.UserID));
				Assert.That(userse.UserName, Is.EqualTo(userbe.UserName));
			}
		}

		[Test]
		public void Count()
		{
			DataMigrationHelper.AddUsers(250, DbAccessLayer);

			int runPrimetivSelect = -1;
			int forResult = -1;
			var deSelect = DbAccessLayer.Select<Users>();
			if (DbAccessLayer.DbAccessType == DbAccessType.MsSql)
			{
				runPrimetivSelect = DbAccessLayer.RunPrimetivSelect<int>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))[0];
				forResult = DbAccessLayer.Query().Select<Users>().CountInt().ForResult().FirstOrDefault();
			}
			if (DbAccessLayer.DbAccessType == DbAccessType.SqLite)
			{
				runPrimetivSelect = (int)DbAccessLayer.RunPrimetivSelect<long>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))[0];
				forResult = (int)DbAccessLayer.Query().Select<Users>().CountLong().ForResult().FirstOrDefault();
			}

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult));
			Assert.That(deSelect.Length, Is.EqualTo(forResult));
		}

		[Test]
		public void RefIn()
		{
			var addBooksWithImage = DataMigrationHelper.AddBooksWithImage(250,2, DbAccessLayer);
			foreach (var id in addBooksWithImage)
			{
				int countOfImages = -1;
				if (DbAccessLayer.DbAccessType == DbAccessType.MsSql)
				{
					countOfImages =
					DbAccessLayer.RunPrimetivSelect<int>(string.Format("SELECT COUNT(1) FROM {0} WHERE {0}.{1} = {2}",
					ImageMeta.TableName, ImageMeta.ForgeinKeyName,
					id))[0];
				}
				if (DbAccessLayer.DbAccessType == DbAccessType.SqLite)
				{
					countOfImages =
					(int)DbAccessLayer.RunPrimetivSelect<long>(string.Format("SELECT COUNT(1) FROM {0} WHERE {0}.{1} = {2}",
					ImageMeta.TableName, ImageMeta.ForgeinKeyName,
					id))[0];
				}

				Assert.That(countOfImages, Is.EqualTo(2));
				var deSelect = DbAccessLayer.SelectNative<Image>(string.Format("{2} AS b WHERE b.{0} = {1}", ImageMeta.ForgeinKeyName, id, ImageMeta.SelectStatement));
				Assert.That(deSelect, Is.Not.Empty);
				Assert.That(deSelect.Length, Is.EqualTo(countOfImages));
				var book = DbAccessLayer.Select<Book>(id);
				var forResult = DbAccessLayer.Query().Select<Image>().In(book).ForResult().ToArray();
				Assert.That(forResult, Is.Not.Empty);
				Assert.That(forResult.Count, Is.EqualTo(countOfImages));
			}
		}
		[Category("MsSQL")]
		[Test]
		public void Pager()
		{
			var maxItems = 250;

			DataMigrationHelper.AddUsers(maxItems, DbAccessLayer);
			var basePager = DbAccessLayer.Database.CreatePager<Users>();
			basePager.PageSize = 10;
			basePager.LoadPage(DbAccessLayer);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(maxItems / basePager.PageSize));

			var queryPager = DbAccessLayer.Query().Select<Users>().Order().By(f => f.UserID).ForPagedResult(1, basePager.PageSize);
			queryPager.LoadPage(DbAccessLayer);

			Assert.That(basePager.CurrentPage, Is.EqualTo(queryPager.CurrentPage));
			Assert.That(basePager.MaxPage, Is.EqualTo(queryPager.MaxPage));
		}

		[Test]
		public void PagerWithCondtion()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccessLayer);


			var basePager = DbAccessLayer.Database.CreatePager<Users>();
			basePager.BaseQuery = DbAccessLayer.CreateSelect<Users>(" WHERE User_ID < 25");
			basePager.PageSize = 10;
			basePager.LoadPage(DbAccessLayer);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(Math.Ceiling(25F / basePager.PageSize)));

			var queryPager = DbAccessLayer.Query().Select<Users>()
				.Where()
				.Column(f => f.UserID)
				.IsQueryOperatorValue("< 25")
				.Order().By(f => f.UserID)
				.ForPagedResult(1, basePager.PageSize);
			queryPager.LoadPage(DbAccessLayer);

			Assert.That(basePager.CurrentPage, Is.EqualTo(queryPager.CurrentPage));
			Assert.That(basePager.MaxPage, Is.EqualTo(queryPager.MaxPage));
		}


		[Category("MsSQL")]
		[Test]
		public void AsCte()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccessLayer);
			var elementProducer = DbAccessLayer.Query().Select<Users>().AsCte<Users, Users>("cte");
			var query = elementProducer.ContainerObject.Compile();
		}


		[Test]
		public void OrderBy()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccessLayer);
			var elementProducer = DbAccessLayer.Query().Select<Users>().Order().By(s => s.UserName).ThenBy(f => f.UserID).ToArray();
			var directQuery = DbAccessLayer.SelectNative<Users>(UsersMeta.SelectStatement + " ORDER BY UserName, User_ID");

			Assert.That(directQuery.Length, Is.EqualTo(elementProducer.Length));

			for (int index = 0; index < directQuery.Length; index++)
			{
				var userse = directQuery[index];
				var userbe = elementProducer[index];
				Assert.That(userbe.UserID, Is.EqualTo(userse.UserID));
				Assert.That(userbe.UserName, Is.EqualTo(userse.UserName));
			}
		}

		[Test]
		public void In()
		{
			var maxItems = 250;
			var addUsers = DataMigrationHelper.AddUsers(maxItems, DbAccessLayer);

			var rand = new Random(54541117);

			var elementsToRead = new List<long>();

			for (int i = 0; i < 25; i++)
			{
				elementsToRead.Add(addUsers[rand.Next(0, addUsers.Length)]);
			}

			var elementProducer = DbAccessLayer.Query().Select<Users>().Where().Column(f => f.UserID).Is().In(elementsToRead.ToArray()).ToArray();
			var directQuery = DbAccessLayer.SelectNative<Users>(UsersMeta.SelectStatement
				+ string.Format(" WHERE User_ID IN ({0})",
				elementsToRead.Select(f => f.ToString()).Aggregate((e,f) => e + "," + f)));

			Assert.That(directQuery.Length, Is.EqualTo(elementProducer.Length));

			for (int index = 0; index < directQuery.Length; index++)
			{
				var userse = directQuery[index];
				var userbe = elementProducer[index];
				Assert.That(userbe.UserID, Is.EqualTo(userse.UserID));
				Assert.That(userbe.UserName, Is.EqualTo(userse.UserName));
			}
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void CheckFactory()
		{
			var addUsers = DataMigrationHelper.AddUsers(250, DbAccessLayer);
			Assert.That(() => DbAccessLayer.Query().Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_PK_IDFM_FUNCSELECTFACWITHPARAM testUser = null;
			Assert.That(() => testUser = DbAccessLayer.InsertWithSelect(new Users_PK_IDFM_FUNCSELECTFACWITHPARAM { UserName = testInsertName }),
				Is.Not.Null
				.And.Property("UserId").Not.EqualTo(0));

			var selTestUser = DbAccessLayer.Query().Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] { testUser.UserId }).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.That(selTestUser.UserName, Is.EqualTo(testUser.UserName));
			Assert.That(selTestUser.UserId, Is.EqualTo(testUser.UserId));
		}
	}
}
