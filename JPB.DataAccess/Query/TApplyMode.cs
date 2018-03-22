namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Apply modes for TSQL. This is an helper method that can be used to create APPLYs by using the QueryCommand Builder
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.MsQueryBuilderExtentions.ApplyMode" />
	public class TApplyMode : MsQueryBuilderExtentions.ApplyMode
	{
		/// <summary>
		///     Defines an TSQL Outer Apply statement
		/// </summary>
		public static readonly TApplyMode Outer = new TApplyMode("OUTER APPLY");

		/// <summary>
		///     Defines an TSQL Cross Apply statement
		/// </summary>
		public static readonly TApplyMode Cross = new TApplyMode("CROSS APPLY");

		/// <summary>
		///     Initializes a new instance of the <see cref="TApplyMode" /> class.
		/// </summary>
		/// <param name="applyType">Type of the apply.</param>
		public TApplyMode(string applyType)
				: base(applyType)
		{
		}
	}
}