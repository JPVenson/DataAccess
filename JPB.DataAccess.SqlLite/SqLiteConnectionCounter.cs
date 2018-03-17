using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace JPB.DataAccess.SqLite
{
	public class SqLiteConnectionCounter : IDisposable
	{
		public SqLiteConnectionCounter(string fileName)
		{
			FileName = fileName;
			DbConnectionCounter = new ConditionalWeakTable<IDbConnection, DisposableAction>();
			Connections = new ConcurrentDictionary<WeakReference<IDbConnection>, object>();
		}

		public string FileName { get; private set; }
		private ConditionalWeakTable<IDbConnection, DisposableAction> DbConnectionCounter { get; set; }

		public void AddConnection(IDbConnection connection)
		{
			var refToConnection = new WeakReference<IDbConnection>(connection);
			Connections.TryAdd(refToConnection, new object());

			EventHandler onDisposed = null;
			onDisposed = (sender, obj) =>
			{
				((SQLiteConnection)connection).Disposed -= onDisposed;
				object x;
				Connections.TryRemove(refToConnection, out x);
			};

			((SQLiteConnection) connection).Disposed += onDisposed;

			DbConnectionCounter.Add(connection, new DisposableAction(() => onDisposed(null, null)));
		}

		public ConcurrentDictionary<WeakReference<IDbConnection>, object> Connections { get; private set; }

		public void Dispose()
		{
			foreach (var connection in Connections)
			{
				IDbConnection connectionData;
				if (connection.Key.TryGetTarget(out connectionData))
				{
					if (connectionData.State == ConnectionState.Open)
					{
						connectionData.Close();
						connectionData.Dispose();
					}
				}
			}
		}
	}
}