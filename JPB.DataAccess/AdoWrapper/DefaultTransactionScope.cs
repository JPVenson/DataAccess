using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.AdoWrapper
{
	public class DefaultTransactionScope : IDisposable
	{
		private readonly IDatabase _db;

		public DefaultTransactionScope(IDatabase db)
		{
			_db = db;
		}

		public void Dispose()
		{
			_db.CloseConnection();
		}
	}
}
