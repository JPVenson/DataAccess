using System.Collections;
using System.Collections.Generic;

namespace JPB.DataAccess.Manager
{
    public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbTypes, string>>
    {
        private readonly Dictionary<DbTypes, string> _preDefinedProvider = new Dictionary<DbTypes, string>
        {
            {DbTypes.MsSql, "JPB.DataAccess.AdoWrapper.MsSql.MsSql"},
            {DbTypes.OleDb, "JPB.DataAccess.AdoWrapper.OleDB.OleDb"},
            {DbTypes.Obdc, "JPB.DataAccess.AdoWrapper.Obdc.Obdc"},
            {DbTypes.MySql, "JPB.DataAccess.MySql.MySql"},
            {DbTypes.SqLite, "JPB.DataAccess.SqlLite.SqLite"},
        };

        public IEnumerator<KeyValuePair<DbTypes, string>> GetEnumerator()
        {
            return _preDefinedProvider.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _preDefinedProvider.Count; }
        }
    }
}