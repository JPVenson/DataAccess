using System.Data;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <inheritdoc />
	public class RemotingDbParameter : IDbDataParameter
	{
		/// <summary>
		///		The Associated Strategy
		/// </summary>
		public RemotingStrategy Strategy { get; }
		/// <inheritdoc />
		public RemotingDbParameter(RemotingStrategy strategy)
		{
			Strategy = strategy;
			Strategy.Events.OnParameterCreated(this);
		}
		
		/// <inheritdoc />
		public DbType DbType { get; set; }
		/// <inheritdoc />
		public ParameterDirection Direction { get; set; }
		/// <inheritdoc />
		public bool IsNullable { get; set; }
		/// <inheritdoc />
		public string ParameterName { get; set; }
		/// <inheritdoc />
		public string SourceColumn { get; set; }
		/// <inheritdoc />
		public DataRowVersion SourceVersion { get; set; }
		/// <inheritdoc />
		public object Value { get; set; }
		/// <inheritdoc />
		public byte Precision { get; set; }
		/// <inheritdoc />
		public byte Scale { get; set; }
		/// <inheritdoc />
		public int Size { get; set; }
	}
}