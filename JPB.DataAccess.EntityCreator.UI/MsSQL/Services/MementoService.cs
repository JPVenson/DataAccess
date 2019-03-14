using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public class MementoService
	{
		private static MementoService _instance;

		private MementoService()
		{
			Actions = new ConcurrentStack<IMementoAction>();
		}

		public static MementoService Instance
		{
			get { return _instance ?? (_instance = new MementoService()); }
		}

		public ConcurrentStack<IMementoAction> Actions { get; set; }

		public void SetOption(IMementoAction action)
		{
			Actions.Push(action);
		}
	}
}
