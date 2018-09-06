using System;
using System.Collections.Concurrent;
using JPB.DataAccess.EntityCreator.UI.MsSQL.Services;

namespace JPB.DataAccess.EntityCreator.UI.Shared.Model
{
	[Serializable]
	public class ConfigStore
	{
		public ConcurrentStack<IMementoAction> Actions { get; set; }
		public string SourceConnectionString { get; set; }
		public string Version { get; set; }
	}
}
