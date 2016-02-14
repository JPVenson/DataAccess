using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JPB.DataAccess.UnitTests.DbCollectionTests
{
	[TestClass]
	public class DbCollection
	{
		private DbCollection<Users_Col> _dbCollection;

		public DbCollection()
		{

		}

		[TestInitialize]
		public void Init()
		{
			_dbCollection = new DbCollection<Users_Col>(new Users_Col[0]);
			Assert.IsNotNull(_dbCollection);
		}

		[TestMethod]
		public void Add()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);
		}

		[TestMethod]
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

		[TestMethod]
		public void Contains()
		{
			var user = new Users_Col();
			_dbCollection.Add(user);

			Assert.AreEqual(_dbCollection.Count, 1);
			Assert.AreEqual(_dbCollection.GetEntryState(user), CollectionStates.Added);
			Assert.AreEqual(_dbCollection.Contains(user), true);
		}

		[TestMethod]
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

		//[TestMethod]
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
