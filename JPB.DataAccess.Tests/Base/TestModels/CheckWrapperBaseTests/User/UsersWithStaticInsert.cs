#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	public class UsersWithStaticInsert
	{
		public long User_ID { get; set; }
		public string UserName { get; set; }

		[InsertFactoryMethod(TargetDatabase = DbAccessType.MsSql)]
		[InsertFactoryMethod(TargetDatabase = DbAccessType.MySql)]
		[InsertFactoryMethod(TargetDatabase = DbAccessType.MsSql | DbAccessType.Remoting)]
		[InsertFactoryMethod(TargetDatabase = DbAccessType.MySql | DbAccessType.Remoting)]
		public string Insert()
		{
			return string.Format("INSERT INTO {0}({2}) VALUES ('{1}')", UsersMeta.TableName, UserName,
				UsersMeta.ContentName);
		}

		[InsertFactoryMethod(TargetDatabase = DbAccessType.SqLite)]
		[InsertFactoryMethod(TargetDatabase = DbAccessType.SqLite | DbAccessType.Remoting)]
		public string InsertSqLite()
		{
			return string.Format("INSERT INTO {0} VALUES (0, '{1}')", UsersMeta.TableName, UserName);
		}
	}
}