using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.AdoWrapper.Remoting;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MySql;
using JPB.DataAccess.SqLite;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.Overwrite.Framework
{
	public class RemotingManager : IManagerImplementation
	{
		private readonly IManagerImplementation _subManager;

		public RemotingManager(IManagerImplementation subManager)
		{
			_subManager = subManager;
		}

		public DbAccessType DbAccessType
		{
			get { return _subManager.DbAccessType | DbAccessType.Remoting; }
		}

		public string ConnectionString
		{
			get { return _subManager.ConnectionString; }
		}

		public class LocalRemotingStrategy : RemotingStrategyExternal
		{
			private readonly RemotingManager _manager;
			public RemotingConsumerServer RemotingConsumerServer { get; set; }

			public LocalRemotingStrategy(DbAccessType emulateDbType, DbConfig config, RemotingManager manager,
				string instanceName) : base(emulateDbType, config)
			{
				_manager = manager;
				var wrapper = _manager._subManager.GetWrapper(_manager._subManager.DbAccessType, instanceName);
				RemotingConsumerServer = new RemotingConsumerServer(() =>
				{
					return wrapper;
				});
			}

			public override object Clone()
			{
				throw new NotImplementedException();
			}

			public override IDataPager<T> CreatePager<T>()
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.Experimental:
						break;
					case DbAccessType.Unknown:
						break;
					case DbAccessType.MsSql:
						return new MsSqlDataPager<T>();
					case DbAccessType.MySql:
						return new MySqlDataPager<T>();
					case DbAccessType.OleDb:
						break;
					case DbAccessType.Obdc:
						break;
					case DbAccessType.SqLite:
						return new SqLiteDataPager<T>();
					case DbAccessType.Remoting:
						break;
				}
				throw new ArgumentOutOfRangeException();
			}

			public override IDbCommand EnableIdentityInsert(string classInfoTableName, IDbConnection conn)
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.MsSql:
						return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] ON", classInfoTableName), conn);
					case DbAccessType.MySql:
						return null;
					case DbAccessType.SqLite:
						return null;
				}
				throw new NotImplementedException();
			}

			public override IDbCommand DisableIdentityInsert(string classInfoTableName, IDbConnection conn)
			{
				switch (_manager._subManager.DbAccessType)
				{
					case DbAccessType.MsSql:
						return CreateCommand(string.Format("SET IDENTITY_INSERT [{0}] OFF", classInfoTableName), conn);
					case DbAccessType.MySql:
						return null;
					case DbAccessType.SqLite:
						return null;
				}
				throw new NotImplementedException();
			}

			public override string RegisterConnection()
			{
				return RemotingConsumerServer.RegisterConnection();
			}

			public override void CloseConnection(string connectionId)
			{
				RemotingConsumerServer.CloseConnection(connectionId);
			}

			public override string RegisterTransaction(string connectionId)
			{
				return RemotingConsumerServer.RegisterTransaction(connectionId);
			}

			public override bool RollbackTransaction(string connectionId, string transactionId)
			{
				return RemotingConsumerServer.RollbackTransaction(connectionId, transactionId);
			}

			public override bool CommitTransaction(string connectionId, string transactionId)
			{
				return RemotingConsumerServer.CommitTransaction(connectionId, transactionId);
			}

			public override int ExecuteQuery(string commandGraph, string connectionId, string transactionId)
			{
				return RemotingConsumerServer.ExecuteQuery(commandGraph, connectionId, transactionId);
			}

			public override object ExecuteScalar(string commandGraph, string connectionId, string transactionId)
			{
				return RemotingConsumerServer.ExecuteScalar(commandGraph, connectionId, transactionId);
			}

			public class DataReaderRemoteData : IEnumerable<IEnumerable<IDataRecord>>, IXmlSerializable
			{
				public DataReaderRemoteData()
				{
					Data = new List<IEnumerable<IDataRecord>>();
				}
				public IEnumerable<IEnumerable<IDataRecord>> Data { get; set; }
				public IEnumerator<IEnumerable<IDataRecord>> GetEnumerator()
				{
					return Data.GetEnumerator();
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return ((IEnumerable)Data).GetEnumerator();
				}

				public XmlSchema GetSchema()
				{
					return null;
				}

				public void ReadXml(XmlReader reader)
				{
					var listOfListOfRecords = new List<List<EagarDataRecord>>();
					if (!reader.IsEmptyElement)
					{
						reader.ReadStartElement();//<lrec>
						while (reader.Name == "lrec")
						{
							var listOfRecords = new List<EagarDataRecord>();
							if (!reader.IsEmptyElement)
							{
								reader.ReadStartElement();//<rec>
								while (reader.Name == "rec")
								{
									var record = new EagarDataRecord();
									record.ReadXml(reader);
									listOfRecords.Add(record);
									reader.ReadEndElement();//<rec>
								}
								reader.ReadEndElement();//</lrec>	
							}
							else
							{
								reader.ReadStartElement();//<lrec>
							}
							listOfListOfRecords.Add(listOfRecords);	
						}
					}

					Data = listOfListOfRecords;
				}

				public void WriteXml(XmlWriter writer)
				{
					foreach (var records in Data)
					{
						writer.WriteStartElement("lrec");
						foreach (var dataRecord in records)
						{
							writer.WriteStartElement("rec");
							if (dataRecord is IXmlSerializable xmlSerializable)
							{
								xmlSerializable.WriteXml(writer);
							}
							else
							{
								writer.WriteValue(dataRecord);
							}
							writer.WriteEndElement();//</rec>
						}
						writer.WriteEndElement();//</lrec>
					}
				}
			}

			public override IEnumerable<IEnumerable<IDataRecord>> ExecuteCommand(string commandGraph, string connectionId, string transactionId, out int recordsAffected)
			{
				var enumerateCommand = RemotingConsumerServer.EnumerateCommand(commandGraph, connectionId, transactionId, out recordsAffected);

				var xmlSerializer = new XmlSerializer(typeof(DataReaderRemoteData));
				using (var ms = new MemoryStream())
				{
					xmlSerializer.Serialize(ms, new DataReaderRemoteData()
					{
						Data = enumerateCommand
					});
					var xmlText = Encoding.Default.GetString(ms.ToArray());
					ms.Seek(0, SeekOrigin.Begin);
					var commandResultCopy = xmlSerializer.Deserialize(ms) as DataReaderRemoteData;
					return commandResultCopy;
				}
			}
		}

		private LocalRemotingStrategy _strategy;

		public DbAccessLayer GetWrapper(DbAccessType type, string instanceName)
		{
			return new DbAccessLayer(_strategy = new LocalRemotingStrategy(type, new DbConfig(), this, instanceName + "RMT_"));
		}

		public void FlushErrorData()
		{
			_subManager.FlushErrorData();
		}

		public void Clear()
		{
			if (_strategy?.RemotingConsumerServer.HasOpenConnection() == true)
			{
				Assert.Fail("There are still open Connections");
			}
			_subManager.Clear();
		}
	}
}