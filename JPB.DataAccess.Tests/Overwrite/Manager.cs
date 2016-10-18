using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;
using NUnit.Framework;

namespace JPB.DataAccess.Tests
{
	public class Manager : IManager
	{
		static Manager()
		{
			_managers = new Dictionary<DbAccessType, IManager>();
			_managers.Add(DbAccessType.MsSql, new MsSqlManager());
			_managers.Add(DbAccessType.SqLite, new SqLiteManager());
			_managers.Add(DbAccessType.MySql, new MySqlManager());
		}

		private static Dictionary<DbAccessType, IManager> _managers;

		public static void AddManager(DbAccessType type, IManager manager)
		{
			_managers.Add(type, manager);
		}

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

		public DbAccessLayer GetWrapper(DbAccessType type)
		{
			DbAccessLayer expectWrapper = null;
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("Element type Lookup");
			Console.WriteLine("Found " + type);
			expectWrapper = _managers[type].GetWrapper(type);

			//if (elementType == DbAccessType.MsSql)
			//{
			//	expectWrapper = new MsSqlManager().GetWrapper();
			//}
			//else if (elementType == DbAccessType.SqLite)
			//{
			//	expectWrapper = new SqLiteManager().GetWrapper();
			//}

			_errorData = new StringBuilder();
			expectWrapper.RaiseEvents = true;
			expectWrapper.OnSelect += (sender, eventArg) =>
			{
				_errorData.AppendFormat(@"SELECT: \r\n{0}", eventArg.QueryDebugger);
				_errorData.AppendLine();
			};

			expectWrapper.OnDelete += (sender, eventArg) =>
			{
				_errorData.AppendFormat(@"DELETE: \r\n{0}", eventArg.QueryDebugger);
				_errorData.AppendLine();
			};

			expectWrapper.OnInsert += (sender, eventArg) =>
			{
				_errorData.AppendFormat(@"INSERT: \r\n{0}", eventArg.QueryDebugger);
				_errorData.AppendLine();
			};

			expectWrapper.OnUpdate += (sender, eventArg) =>
			{
				_errorData.AppendFormat(@"UPDATE: \r\n{0}", eventArg.QueryDebugger);
				_errorData.AppendLine();
			};

			Assert.NotNull(expectWrapper, "This test cannot run as no Database Variable is defined");
			bool checkDatabase = expectWrapper.CheckDatabase();
			Assert.IsTrue(checkDatabase);

			expectWrapper.Config.ConstructorSettings.FileCollisonDetection = CollisonDetectionMode.Pessimistic;
			expectWrapper.Config.ConstructorSettings.CreateDebugCode = false;
			expectWrapper.Multipath = true;
			QueryDebugger.UseDefaultDatabase = expectWrapper.DatabaseStrategy;
			return expectWrapper;
		}

		public DbAccessType DbAccessType { get; }
		public string ConnectionString { get; }

		private StringBuilder _errorData;

		public void FlushErrorData()
		{
			Console.WriteLine(_errorData.ToString());
			_errorData.Clear();
		}
	}
}