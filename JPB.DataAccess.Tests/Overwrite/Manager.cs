using System;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
using NUnit.Framework;

#if SqLite
using System.IO;
#endif

namespace JPB.DataAccess.Tests
{
	public interface IManager
	{
		DbAccessLayer GetWrapper();
	}

	public class Manager : IManager
	{
		public DbAccessType GetElementType()
		{
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("Element type Lookup");
			var customAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<CategoryAttribute>();
			if (customAttribute.Name == "MsSQL")
			{
				Console.WriteLine("Found MsSQL");
				return DbAccessType.MsSql;
			}
			else if (customAttribute.Name == "SqLite")
			{
				Console.WriteLine("Found SqLite");
				return DbAccessType.SqLite;
			}
			Console.WriteLine("Found NON ERROR");
			return DbAccessType.Unknown;
		}

		public DbAccessLayer GetWrapper()
		{
			DbAccessLayer expectWrapper = null;
			var elementType = GetElementType();
			if (elementType == DbAccessType.MsSql)
			{
				expectWrapper = new MsSqlManager().GetWrapper();
			}
			else if (elementType == DbAccessType.SqLite)
			{
				expectWrapper = new SqLiteManager().GetWrapper();
			}

			expectWrapper.RaiseEvents = true;
			expectWrapper.OnSelect += (sender, eventArg) =>
			{
				Console.WriteLine(@"SELECT: \r\n{0}", eventArg.QueryDebugger);
			};

			expectWrapper.OnDelete += (sender, eventArg) =>
			{
				Console.WriteLine(@"DELETE: \r\n{0}", eventArg.QueryDebugger);
			};

			expectWrapper.OnInsert += (sender, eventArg) =>
			{
				Console.WriteLine(@"INSERT: \r\n{0}", eventArg.QueryDebugger);
			};

			expectWrapper.OnUpdate += (sender, eventArg) =>
			{
				Console.WriteLine(@"Update: \r\n{0}", eventArg.QueryDebugger);
			};

			Assert.NotNull(expectWrapper, "This test cannot run as no Database Variable is defined");
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			DbConfig.ConstructorSettings.CreateDebugCode = false;
			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}
	}
}