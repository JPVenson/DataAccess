using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.DebuggerHelper
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Query : {DebuggerQuery}", Name = "Query")]
    public class QueryDebugger
    {
        static QueryDebugger()
        {
            Assembly = Assembly.GetAssembly(typeof(QueryDebugger));
        }

        static readonly Assembly Assembly;

        public string DebuggerQuery { get; private set; }
        public string SqlQuery { get; private set; }
        public string StackTracer { get; private set; }

        public const string StartValuePart = @" {{";
        public const string EndValuePart = @"}} ";

        public QueryDebugger(IDbCommand command)
        {
            var frames = new StackTrace().GetFrames();
            IEnumerable<StackFrame> stackFrames;
            if (frames != null)
            {
                stackFrames = frames.Where(s =>
                {
                    var methodBase = s.GetMethod();
                    if (Assembly.DefinedTypes.Contains(methodBase.DeclaringType))
                        return false;

                    if (methodBase.DeclaringType != null && methodBase.DeclaringType.Assembly.GlobalAssemblyCache)
                        return false;

                    return true;
                });
            }
            else
            {
                stackFrames = new List<StackFrame>();
            }
            var enumerable = stackFrames.Select(s => s.ToString()).ToArray();
            if (enumerable.Any())
                StackTracer = enumerable.Aggregate((e, f) => e + Environment.NewLine + f);
            var debugquery = new StringBuilder(command.CommandText);
            var sqlReady = CommandAsMsSql(command);
            DebuggerQuery = debugquery.ToString();
            SqlQuery = sqlReady;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static String ParameterValue(IDataParameter sp)
        {
            String retval = "";

            switch (sp.DbType)
            {
                case DbType.VarNumeric:
                case DbType.Decimal:
                case DbType.Currency:
                case DbType.AnsiStringFixedLength:
                case DbType.Time:
                case DbType.AnsiString:
                case DbType.Xml:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                    break;

                case DbType.Boolean:
                    retval = (sp.Value is bool && (bool)sp.Value) ? "1" : "0";
                    break;

                default:
                    retval = sp.Value.ToString().Replace("'", "''");
                    break;
            }

            return retval;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static String CommandAsMsSql(IDbCommand sc)
        {
            if (!(sc is SqlCommand))
                return sc.CommandText;

            var sql = new StringBuilder();
            Boolean firstParam = true;

            if (!string.IsNullOrEmpty(sc.Connection.Database))
                sql.AppendLine("USE " + sc.Connection.Database + ";");

            switch (sc.CommandType)
            {
                case CommandType.StoredProcedure:
                    sql.AppendLine("DECLARE @return_value int;");

                    foreach (var sp in sc.Parameters.Cast<SqlParameter>())
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.Append("DECLARE " + sp.ParameterName + "\t" + sp.SqlDbType + "\t= ");

                            sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "NULL" : ParameterValue(sp)) + ";");

                        }
                    }

                    sql.AppendLine("EXEC [" + sc.CommandText + "]");

                    foreach (var sp in sc.Parameters.Cast<IDataParameter>())
                    {
                        if (sp.Direction != ParameterDirection.ReturnValue)
                        {
                            sql.Append((firstParam) ? "\t" : "\t, ");

                            if (firstParam) firstParam = false;

                            if (sp.Direction == ParameterDirection.Input)
                                sql.AppendLine(sp.ParameterName + " = " + ParameterValue(sp));
                            else

                                sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " OUTPUT");
                        }
                    }
                    sql.AppendLine(";");

                    sql.AppendLine("SELECT 'Return Value' = CONVERT(NVARCHAR, @return_value);");

                    foreach (var sp in sc.Parameters.Cast<IDataParameter>())
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.AppendLine("SELECT '" + sp.ParameterName + "' = CONVERT(NVARCHAR, " + sp.ParameterName + ");");
                        }
                    }
                    break;
                case CommandType.Text:
                    foreach (var sp in sc.Parameters.Cast<SqlParameter>())
                    {
                        sql.AppendLine("DECLARE " + " @" + sp.ParameterName + " " + sp.SqlDbType + " = " + ParameterValue(sp) + ";");
                    }

                    sql.AppendLine(sc.CommandText);
                    break;
            }

            return sql.ToString();
        }
    }
}
