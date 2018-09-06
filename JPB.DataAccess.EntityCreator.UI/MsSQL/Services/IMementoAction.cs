using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public interface IMementoAction
	{
		void Replay(SqlEntityCreatorViewModel creator);
	}
}