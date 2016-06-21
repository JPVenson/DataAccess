using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests.Overwrite
{
	public class DataMigrationHelper
	{
		public static void AddUsers(int number)
		{
			var mgr = new Manager().GetWrapper();
			for (int i = 0; i < number; i++)
			{
				var user = new Users();
				user.UserName = Guid.NewGuid().ToString();
				mgr.Insert(user);
			}
		}
	}
}
