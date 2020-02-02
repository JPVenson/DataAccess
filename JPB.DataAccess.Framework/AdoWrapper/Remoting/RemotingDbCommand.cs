using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace JPB.DataAccess.AdoWrapper.Remoting
{
	/// <summary>
	///		Wraps a command that can be executed external
	/// </summary>
	public class RemotingDbCommand : IDbCommand
	{
		/// <summary>
		///		The Associated Strategy
		/// </summary>
		public RemotingStrategy Strategy { get; }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="strategy"></param>
		public RemotingDbCommand(RemotingStrategy strategy)
		{
			Strategy = strategy;
			Parameters = new RemotingParameterCollection();
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}
		
		/// <inheritdoc />
		public void Cancel()
		{
			Strategy.Events.OnCommandCancel(this);
		}
		
		/// <inheritdoc />
		public IDbDataParameter CreateParameter()
		{
			var param = new RemotingDbParameter(Strategy);
			return param;
		}
		
		/// <inheritdoc />
		public int ExecuteNonQuery()
		{
			return Strategy.ExecuteQuery(this);
		}
		
		/// <inheritdoc />
		public IDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}
		
		/// <inheritdoc />
		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return Strategy.ExecuteReader(this, behavior);
		}
		
		/// <inheritdoc />
		public object ExecuteScalar()
		{
			return Strategy.ExecuteScalar(this);
		}
		
		/// <inheritdoc />
		public void Prepare()
		{
			Strategy.Events.OnCommandPrepare(this);
		}
		
		/// <inheritdoc />
		public string CommandText { get; set; }
		/// <inheritdoc />
		public int CommandTimeout { get; set; }
		/// <inheritdoc />
		public CommandType CommandType { get; set; }
		/// <inheritdoc />
		public IDbConnection Connection { get; set; }
		/// <inheritdoc />
		public IDataParameterCollection Parameters { get; }
		/// <inheritdoc />
		public IDbTransaction Transaction { get; set; }
		/// <inheritdoc />
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