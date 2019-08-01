using System;
using System.Data;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.Query.Contracts;

namespace JPB.DataAccess.Framework.Query.Operators
{
	internal class EventPostProcessor : IQueryCommandInterceptor
	{
		private readonly EventType _handler;
		private readonly DbAccessLayer _source;

		internal enum EventType
		{
			Select,
			Insert,
			Delete,
			Update,
			Non
		}

		internal EventPostProcessor(EventType handler, DbAccessLayer source)
		{
			_handler = handler;
			_source = source;
		}


		public IDbCommand QueryExecuting(IDbCommand command)
		{
			switch (_handler)
			{
				case EventType.Select:
					_source.RaiseSelect(command);
					break;
				case EventType.Insert:
					_source.RaiseInsert(this, command);
					break;
				case EventType.Delete:
					_source.RaiseDelete(this, command);
					break;
				case EventType.Update:
					_source.RaiseUpdate(this, command);
					break;
				case EventType.Non:
					_source.RaiseNoResult(this, command);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return command;
		}

		public IDbCommand NonQueryExecuting(IDbCommand command)
		{
			switch (_handler)
			{
				case EventType.Select:
					_source.RaiseSelect(command);
					break;
				case EventType.Insert:
					_source.RaiseInsert(this, command);
					break;
				case EventType.Delete:
					_source.RaiseDelete(this, command);
					break;
				case EventType.Update:
					_source.RaiseUpdate(this, command);
					break;
				case EventType.Non:
					_source.RaiseNoResult(this, command);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return command;
		}
	}
}