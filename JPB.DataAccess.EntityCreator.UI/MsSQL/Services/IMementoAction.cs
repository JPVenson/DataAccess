using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public interface IMementoAction
	{
		void Replay(SqlEntityCreatorViewModel creator);
	}

	public class SetPropertyAction : IMementoAction
	{
		public SetPropertyAction()
		{
			
		}

		public void Replay(SqlEntityCreatorViewModel creator)
		{
			throw new System.NotImplementedException();
		}
	}
}