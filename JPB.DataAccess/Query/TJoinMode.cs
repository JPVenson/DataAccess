#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Jon modes for TSQL. This is an helper method that can be used to create JOINs by using the QueryCommand Builder
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public class TJoinMode : MsQueryBuilderExtentions.JoinMode
	{
		private static IEnumerable<TJoinMode> _joints;

		private TJoinMode(string joinType)
			: base(joinType)
		{
		}

		/// <summary>
		///     Returns a list of all Join values known be the system
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<MsQueryBuilderExtentions.JoinMode> GetJoins()
		{
			if (_joints != null)
				return _joints;

			_joints =
				typeof(TJoinMode)
					.GetFields(BindingFlags.Static)
					.Select(s => s.GetValue(null))
					.Cast<TJoinMode>();
			return _joints;
		}
#pragma warning disable 1591
		public static readonly TJoinMode Left = new TJoinMode("LEFT");
		public static readonly TJoinMode LeftOuter = new TJoinMode("LEFT OUTER");
		public static readonly TJoinMode Right = new TJoinMode("RIGHT");
		public static readonly TJoinMode RightOuter = new TJoinMode("RIGHT OUTER");
		public static readonly TJoinMode Inner = new TJoinMode("INNER");
		public static readonly TJoinMode Outer = new TJoinMode("OUTER");
		public static readonly TJoinMode Cross = new TJoinMode("CROSS");
		public static readonly TJoinMode Full = new TJoinMode("FULL");
		public static readonly TJoinMode FullOuter = new TJoinMode("FULL OUTER");
		public static readonly TJoinMode Self = new TJoinMode("SELF");
#pragma warning restore 1591
	}

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