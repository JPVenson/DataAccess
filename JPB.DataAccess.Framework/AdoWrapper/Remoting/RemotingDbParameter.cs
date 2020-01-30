using System.Data;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public class RemotingDbParameter : IDbDataParameter
	{
		public RemotingStrategy Strategy { get; }
		public RemotingDbParameter(RemotingStrategy strategy)
		{
			Strategy = strategy;
			Strategy.Events.OnParameterCreated(this);
		}

		public DbType DbType { get; set; }
		public ParameterDirection Direction { get; set; }
		public bool IsNullable { get; set; }
		public string ParameterName { get; set; }
		public string SourceColumn { get; set; }
		public DataRowVersion SourceVersion { get; set; }
		public object Value { get; set; }
		public byte Precision { get; set; }
		public byte Scale { get; set; }
		public int Size { get; set; }
	}
}