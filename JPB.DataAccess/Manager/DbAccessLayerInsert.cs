using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        public void Insert<T>(T entry)
        {
            Insert(entry, Database);
        }

        public T InsertWithSelect<T>(T entry)
        {
            return InsertWithSelect(entry, Database);
        }

        public void InsertRange<T>(IEnumerable<T> entry)
        {
            Database.RunInTransaction(s =>
            {
                foreach (T item in entry)
                    Insert(item, s);
            });
        }

        public static IDbCommand _CreateInsert<T>(T entry, IDatabase db)
        {
            Type type = typeof(T);
            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes(false).Any(e => e is InsertIgnore || e is IgnoreReflectionAttribute))
                    .Select(s => s.Name)
                    .Concat(CreateIgnoreList(type))
                    .ToArray();
            string[] propertyInfos = CreatePropertyNames<T>(ignore).ToArray();
            string csvprops = CreatePropertyCSV<T>(ignore);

            string values = "";
            for (int index = 0; index < propertyInfos.Length; index++)
                values = values + ("@" + index + ",");
            values = values.Remove(values.Length - 1);
            string query = "INSERT INTO " + type.GetTableName() + " ( " + csvprops + " ) VALUES ( " + values + " )";

            string[] orignialProps = type.GetPropertysViaRefection(ignore).ToArray();

            return CreateCommandWithParameterValues(query, orignialProps, entry, db);
        }

        public static IDbCommand CreateInsert<T>(T entry, IDatabase db)
        {
            return CheckInstanceForAttriute<T, InsertFactoryMethodAttribute>(entry, db, _CreateInsert);
        }

        public static void Insert<T>(T entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateInsert(entry, s)); });
        }

        public static T InsertWithSelect<T>(T entry, IDatabase db)
        {
            return db.Run(s =>
            {
                IDbCommand dbCommand = CreateInsert(entry, s);
                dbCommand.ExecuteNonQuery();
                object getlastInsertedId = s.GetlastInsertedID();
                return Select<T>(Convert.ToInt64(getlastInsertedId), s);
            });
        }
    }
}