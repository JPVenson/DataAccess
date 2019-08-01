#region

using System;
using System.Linq;
using JPB.DataAccess.Framework.Helper.LocalDb.Scopes;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Self)]
	public class UpdateRefreshTests : DatabaseBaseTest
	{

		[Test]
		public void InsertIdentity()
		{
			Assert.That(() => DbAccess.Database.RunInTransaction(f =>
			{
				using (DbReposetoryIdentityInsertScope.CreateOrObtain())
				{
					DbAccess.Insert(new Users() { UserID = 999999, UserName = "TEST" });
					DbAccess.Query().Update.Table<Users>().Set.Column(g => g.UserID).Value(666).ExecuteNonQuery();
				}
			}), Throws.Exception);
		}

		[Test]
		public void Refresh()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);

			var singleEntity = DbAccess
				.Query()
				.Select.Table<Users>()
				.LimitBy(1)
				.Single();

			var id = singleEntity.UserID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			DbAccess.Update(singleEntity);
			singleEntity.UserName = null;

			singleEntity = DbAccess.Refresh(singleEntity);
			var refEntity = DbAccess.SelectSingle<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		public void RefreshInplace()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var singleEntity = DbAccess
				.Query()
				.Select.Table<Users>()
				.LimitBy(1)
				.Single();
			var id = singleEntity.UserID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			DbAccess.Update(singleEntity);
			singleEntity.UserName = null;

			DbAccess.RefreshKeepObject(singleEntity);
			var refEntity = DbAccess.SelectSingle<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		public void Update()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var query = DbAccess
				.Query()
				.Select
				.Table<Users>()
				.LimitBy(1);
			var singleEntity = query
				.Single();
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			DbAccess.Update(singleEntity);

			var refEntity = DbAccess.SelectSingle<Users>(singleEntity.UserID);
			Assert.IsNotNull(refEntity);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		/// <inheritdoc />
		public UpdateRefreshTests(DbAccessType type, bool asyncExecution, bool syncronised) : base(type, asyncExecution, syncronised)
		{
		}
	}
}