using System.Collections;
using System.Collections.Generic;

namespace JPB.DataAccess.Manager
{
    public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbAccessType, string>>
    {
        private readonly Dictionary<DbAccessType, string> _preDefinedProvider = new Dictionary<DbAccessType, string>
        {
            {DbAccessType.MsSql, "JPB.DataAccess.AdoWrapper.MsSql.MsSql"},
            {DbAccessType.OleDb, "JPB.DataAccess.AdoWrapper.OleDB.OleDb"},
            {DbAccessType.Obdc, "JPB.DataAccess.AdoWrapper.Obdc.Obdc"},
            {DbAccessType.MySql, "JPB.DataAccess.MySql.MySql"},
            {DbAccessType.SqLite, "JPB.DataAccess.SqlLite.SqLite"},
        };

        public IEnumerator<KeyValuePair<DbAccessType, string>> GetEnumerator()
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