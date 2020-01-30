using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	public class RemotingDbCommand : IDbCommand
	{
		public RemotingStrategy Strategy { get; }

		public RemotingDbCommand(RemotingStrategy strategy)
		{
			Strategy = strategy;
			Parameters = new RemotingParameterCollection();
		}

		public void Dispose()
		{

		}

		public void Cancel()
		{
			Strategy.Events.OnCommandCancel(this);
		}

		public IDbDataParameter CreateParameter()
		{
			var param = new RemotingDbParameter(Strategy);
			return param;
		}

		public int ExecuteNonQuery()
		{
			return Strategy.ExecuteQuery(this);
		}

		public IDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return Strategy.ExecuteReader(this, behavior);
		}

		public object ExecuteScalar()
		{
			return Strategy.ExecuteScalar(this);
		}

		public void Prepare()
		{
			Strategy.Events.OnCommandPrepare(this);
		}

		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public IDbConnection Connection { get; set; }
		public IDataParameterCollection Parameters { get; }
		public IDbTransaction Transaction { get; set; }
		public UpdateRowSource UpdatedRowSource { get; set; }

		private class RemotingParameterCollection : Collection<IDbDataParameter>, IDataParameterCollection
		{
			public bool Contains(string parameterName)
			{
				return this[parameterName] != null;
			}

			public int IndexOf(string parameterName)
			{
				for (int i = 0; i < base.Count; i++)
				{
					var item = base[i];
					if (item.ParameterName.Equals(parameterName))
					{
						return i;
					}
				}

				return -1;
			}

			public void RemoveAt(string parameterName)
			{
				Remove(this[parameterName] as IDbDataParameter);
			}

			public object this[string parameterName]
			{
				get
				{
					return this.FirstOrDefault(dataParameter => dataParameter.ParameterName.Equals(parameterName));
				}
				set
				{
					if (Contains(parameterName))
					{
						throw new InvalidOperationException("Cannot set duplicates");
					}
					Add(value as IDbDataParameter);
				}
			}
		}
	}
}