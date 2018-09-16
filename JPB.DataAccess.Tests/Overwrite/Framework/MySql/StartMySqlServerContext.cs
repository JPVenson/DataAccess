using System;
using System.Linq;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite.Framework.MySql
{
	[SetUpFixture]
	public class StartMySqlServerContext
	{
		MySqlConnector.LoggerDelegate loggerDelegate;
		[OneTimeSetUp]
		public void StartMySqlServerEngine()
		{
			loggerDelegate = MySqlConnectorInstance.Instance.AttachLogger();
		}

		[OneTimeTearDown]
		public void StopMySqlServerEngine()
		{
			if (MySqlConnectorInstance.Instance.AwaitMySqlStop == null)
			{
				return;
			}

			Console.WriteLine("Stop MySql Process");
			MySqlConnectorInstance.Instance.StopMySql.Set();
			loggerDelegate.OnLogLine += LoggerDelegate_OnLogLine;
			foreach (var loggerDelegateLogLine in loggerDelegate.LogLines.ToArray())
			{
				TestContext.Out.WriteLine(loggerDelegateLogLine.Orginal);
			}

			Assert.That(MySqlConnectorInstance.Instance.AwaitMySqlStop.Wait((int)TimeSpan.FromSeconds(30).TotalMilliseconds), Is.True, "The MySql server did not stop after 30 secounds");
		}

		private void LoggerDelegate_OnLogLine(object sender, MySqlLogline e)
		{
			TestContext.Out.WriteLine(e.Orginal);
		}
	}
}