/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
				typeof (TJoinMode)
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
	public class TApplyMode : MsQueryBuilderExtentions.ApplyMode
	{
		public static readonly TApplyMode Outer = new TApplyMode("OUTER APPLY");
		public static readonly TApplyMode Cross = new TApplyMode("CROSS APPLY");

		public TApplyMode(string applyType)
			: base(applyType)
		{
		}
	}
}