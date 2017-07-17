#region

using JPB.DataAccess.DbCollection;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.DbCollectionTests
{
	[TestFixture]
	public class DbCollection
	{
		[SetUp]
		public void Init()
		{
			_dbCollection = new DbCollection<Users_Col>(new Users_Col[0]);
			Assert.IsNotNull(_dbCollection);
		}

		private DbCollection<Users_Col> _dbCollection;

		[Test]
		public void Add()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);
		}

		[Test]
		public void Clear()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);

			_dbCollection.Clear();
			Assert.AreEqual(_dbCollection.Count, 0);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Unknown);
		}

		[Test]
		public void Contains()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);
			Assert.AreEqual(_dbCollection.Contains(user), true);
		}

		[Test]
		public void Remove()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);

			_dbCollection.Remove(user);
			Assert.AreEqual(_dbCollection.Count, 0);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Unknown);
		}

		//[Test]
		//public void SaveChangesAddOnly()
		//{
		//	var user = new Users_Col();
		//	_dbCollection.Add(user);

		//	Assert.AreEqual(_dbCollection.Count, 1);
		//	Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);

		//	_dbCollection.SaveChanges()
		//}
	}
}