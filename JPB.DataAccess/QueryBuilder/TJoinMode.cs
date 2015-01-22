using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JPB.DataAccess.QueryBuilder
{
    public class TJoinMode : MsQueryBuilderExtentions.JoinMode
    {
        private TJoinMode(string joinType)
            : base(joinType)
        {
        }

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

        private static IEnumerable<TJoinMode> _joints;

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
    }
}