using System.Linq;
using System.Runtime.CompilerServices;
using JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.Services
{
	public class TableChangePropertyAction : IMementoAction
	{
		private readonly string _currentTableName;

		public TableChangePropertyAction(string currentTableName, object value, [CallerMemberName]string property = null)
		{
			_currentTableName = currentTableName;
			Property = property;
			Value = value;
		}

		public string Property { get; private set; }
		public object Value { get; private set; }

		public void Replay(SqlEntityCreatorViewModel creator)
		{
			var table = creator.Tables.FirstOrDefault(e => e.Info.TableName.Equals(_currentTableName));
			if (table != null)
			{
				table.GetType().GetProperty(Property).SetValue(table, Value);
			}
		}
	}
}