using System.Data;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess
{
    internal static class DataHelperExtensions
    {
        internal static void AddWithValue(this IDataParameterCollection source, string name, object parameter,
            IDatabase db)
        {
            source.Add(db.CreateParameter(name, parameter));
        }
    }
}