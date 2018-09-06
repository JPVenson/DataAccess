using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public class DeleteTableAction : IMementoAction
	{
		private readonly string _newTable;

		public DeleteTableAction(string newTable)
		{
			_newTable = newTable;
		}

		public void Replay(SqlEntityCreatorViewModel creator)
		{
			creator.Tables.Add(new TableInfoViewModel(new TableInfoModel
			{
				Info = new TableInformations
				{
					TableName = _newTable
				}
			}, creator));
		}
	}
}