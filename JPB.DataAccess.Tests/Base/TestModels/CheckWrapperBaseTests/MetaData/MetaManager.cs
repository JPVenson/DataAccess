using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData
{
	public static class MetaManager
	{
		static MetaManager()
		{
			DatabaseMetas = new Dictionary<string, IDatabaseMeta>();
			DatabaseMetas.Add(UsersMeta.TableName, new UsersMeta());
			DatabaseMetas.Add(BookMeta.TableName, new BookMeta());
			DatabaseMetas.Add(ImageMeta.TableName, new ImageMeta());
		}

		public static IDictionary<string, IDatabaseMeta> DatabaseMetas { get; private set; }
	}
}
