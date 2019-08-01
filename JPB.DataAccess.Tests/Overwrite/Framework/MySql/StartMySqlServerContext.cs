using System;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite.Framework.MySql
{
	[SetUpFixture]
	public class StartMySqlServerContext
	{
		[OneTimeSetUp]
		public void StartMySqlServerEngine()
		{
		}

		[OneTimeTearDown]
		public void StopMySqlServerEngine()
		{
			if (MySqlConnectorInstance.Instance.AwaitMySqlStop == null)
			{
				return;
			}
			MySqlConnectorInstance.Instance.StopMySql.Set();
			Assert.That(MySqlConnectorInstance.Instance.AwaitMySqlStop.Wait((int)TimeSpan.FromSeconds(30).TotalMilliseconds), Is.True, "The MySql server did not stop after 30 secounds");
		}
	}
}