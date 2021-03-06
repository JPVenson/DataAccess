﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Manager
{
	internal class DatabaseCommandProcessor : ICommandProcessor
	{
		public EagarDataRecord[][] ExecuteMARSCommand(DbAccessLayer db, IDbCommand query, out int recordsAffected)
		{
			var recordsAffectedA = 0;
			var result = db.Database.Run(
				s =>
				{
					var records = new List<EagarDataRecord[]>();
					using (query)
					{
						query.Connection = query.Connection ?? s.GetConnection();
						query.Transaction = query.Transaction ?? s.ConnectionController.Transaction;
						using (var dr = query.ExecuteReader())
						{
							recordsAffectedA = dr.RecordsAffected;
							try
							{
								do
								{
									var resultSet = new List<EagarDataRecord>();
									while (dr.Read())
									{
										resultSet.Add(EagarDataRecord.WithExcludedFields(dr));
									}

									records.Add(resultSet.ToArray());
								} while (dr.NextResult());
							}
							catch (Exception ex)
							{
								db.RaiseFailedQuery(this, query, ex);
								throw;
							}
						}
					}

					return records.ToArray();
				});
			recordsAffected = recordsAffectedA;
			return result;
		}

		public async Task EnumerateAsync(DbAccessLayer db, IDbCommand command, Action<IDataReader> onRecord, CommandBehavior executionHint = CommandBehavior.Default)
		{
			await db.Database.RunAsync(
				async s =>
				{
					using (command)
					{
						command.Connection = command.Connection ?? s.GetConnection();
						command.Transaction = command.Transaction ?? s.ConnectionController.Transaction;

						IDataReader dr = null;
						try
						{
							var query = command as DbCommand;
							if (query != null && db.Async)
							{
								dr = await query.ExecuteReaderAsync(executionHint).ConfigureAwait(DbAccessLayer.ConfigureAwait);
							}
							else
							{
								dr = command.ExecuteReader(executionHint);
							}

							do
							{
								var reader = dr as DbDataReader;
								while (reader != null && db.Async
									? await reader.ReadAsync().ConfigureAwait(DbAccessLayer.ConfigureAwait)
									: dr.Read())
								{
									onRecord(dr);
								}
							} while (dr.NextResult());
						}
						catch (Exception ex)
						{
							db.RaiseFailedQuery(this, command, ex);
							throw;
						}
						finally
						{
							dr?.Dispose();
						}
					}

					return new object();
				});
		}

		public void Enumerate(DbAccessLayer db, IDbCommand command, Action<IDataReader> onRecord,
			CommandBehavior executionHint = CommandBehavior.Default)
		{
			db.Database.Run(
				s =>
				{
					using (command)
					{
						command.Connection = command.Connection ?? s.GetConnection();
						command.Transaction = command.Transaction ?? s.ConnectionController.Transaction;

						IDataReader dr = null;
						try
						{
							dr = command.ExecuteReader(executionHint);
							do
							{
								while (dr.Read())
								{
									onRecord(dr);
								}
							} while (dr.NextResult());
						}
						catch (Exception ex)
						{
							db.RaiseFailedQuery(this, command, ex);
							throw;
						}
						finally
						{
							dr?.Dispose();
						}
					}

					return new object();
				});
		}

		public async Task<int> ExecuteCommandAsync(DbAccessLayer db, IDbCommand command)
		{
			return await db.Database.RunAsync(async s => await s.ExecuteNonQueryAsync(command, db.Async));
		}

		public int ExecuteCommand(DbAccessLayer db, IDbCommand command)
		{
			return db.Database.Run(s => s.ExecuteNonQuery(command));
		}

		public object GetSkalar(DbAccessLayer db, IDbCommand command, Type requestedType)
		{
			return db.Database.Run(s => s.GetSkalar(command));
		}
	}
}