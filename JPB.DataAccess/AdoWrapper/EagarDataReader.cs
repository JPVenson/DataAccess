#region

using System;
using System.Data;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///     Provides access to the Given object
	/// </summary>
	/// <seealso cref="JPB.DataAccess.AdoWrapper.EgarDataRecord" />
	/// <seealso cref="System.Data.IDataReader" />
	public sealed class EagarDataReader : EgarDataRecord, IDataReader
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EagarDataReader" /> class.
		/// </summary>
		/// <param name="sourceObject">The source object.</param>
		/// <param name="configuration">The access layer.</param>
		internal EagarDataReader(object sourceObject, DbConfig configuration)
		{
			var type = sourceObject.GetType();
			var props = configuration.GetOrCreateClassInfoCache(type).Propertys;
			MetaHeader = props.Select(f => f.Key).ToArray();
			Objects = props.Select(f => f.Value.Getter.Invoke(sourceObject)).ToArray();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EagarDataReader" /> class.
		/// </summary>
		public EagarDataReader(IDataRecord sourceRecord, DbConfig configuration, Type exepctedType)
			: base(sourceRecord, configuration)
		{
		}

		/// <summary>
		///     Closes the <see cref="T:System.Data.IDataReader" /> Object.
		/// </summary>
		public void Close()
		{
		}

		/// <summary>
		///     Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the
		///     <see cref="T:System.Data.IDataReader" />.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Advances the data reader to the next result, when reading the results of batch SQL statements.
		/// </summary>
		/// <returns>
		///     true if there are more rows; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
		/// </summary>
		/// <returns>
		///     true if there are more rows; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Read()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Gets a value indicating the depth of nesting for the current row.
		/// </summary>
		public int Depth { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the data reader is closed.
		/// </summary>
		public bool IsClosed { get; private set; }

		/// <summary>
		///     Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		public int RecordsAffected { get; private set; }
	}
}