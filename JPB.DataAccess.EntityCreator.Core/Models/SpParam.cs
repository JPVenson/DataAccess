using System;
using System.Data;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class SpParam
	{
		public string Parameter { get; set; }
		public SqlDbType Type { get; set; }
	}
}