using System.Linq;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public class CreateTableAction : IMementoAction
	{
		private readonly string _newTable;

		public CreateTableAction(string newTable)
		{
			_newTable = newTable;
		}

		public void Replay(SqlEntityCreatorViewModel creator)
		{
			creator.Tables.Remove(creator.Tables.FirstOrDefault(e => e.Info.TableName.Equals(_newTable)))
		}
	}
}