using System;
using System.Data;
using JPB.DataAccess.DbInfoConfig;

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///		A EgarDataRecord that returns <c>null</c> instedt of <c>DbNull</c> on null fields
	/// </summary>
	public class EgarNullableWrappedRecord : EgarDataRecord
	{
		/// <summary>
		///		ctor
		/// </summary>
		/// <param name="sourceRecord"></param>
		/// <param name="configuration"></param>
		public EgarNullableWrappedRecord(IDataRecord sourceRecord, DbConfig configuration)
				: base(sourceRecord, configuration)
		{

		}

		/// <summary>
		///		Returns Null instead not DbNull
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected internal override object GetValueInternal(int i)
		{
			var val = base.GetValueInternal(i);
			return val == DBNull.Value ? null : val;
		}
	}
}