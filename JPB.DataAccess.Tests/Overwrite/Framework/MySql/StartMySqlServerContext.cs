using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Tests.Overwrite.Framework.MySql;
using NUnit.Framework;

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