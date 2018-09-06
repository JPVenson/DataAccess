using System;
using System.Runtime.CompilerServices;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	[Serializable]
	public class SetPropertyAction : IMementoAction
	{
		public SetPropertyAction(object value, [CallerMemberName]string property = null)
		{
			Property = property;
			Value = value;
		}

		public string Property { get; private set; }
		public object Value { get; private set; }

		public void Replay(SqlEntityCreatorViewModel creator)
		{
			creator.GetType().GetProperty(Property).SetValue(creator, Value);
		}
	}
}