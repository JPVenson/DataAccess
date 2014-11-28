using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;

namespace JPB.DataAccess.DebuggerHelper
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Query : {DebuggerQuery}", Name = "Query")]
    public class QueryDebugger
    {
        /// <summary>
        /// Creates a Debugger that contains some debugging data
        /// </summary>
        /// <param name="command"></param>
        /// <param name="source"></param>
        internal QueryDebugger(IDbCommand command, IDatabase source)
        {
            //Init async because this could be time consuming
            _loaded = false;
            Init();
            var debugquery = new StringBuilder(command.CommandText);
            string formartCommandToQuery;
            if (source != null)
            {
                formartCommandToQuery = source.FormartCommandToQuery(command);
            }
            else
            {
                formartCommandToQuery = this.GenericCommandToQuery(command);
            }

            DebuggerQuery = debugquery.ToString();
            SqlQuery = formartCommandToQuery;
        }

        private string GenericCommandToQuery(IDbCommand command)
        {
            var sql = new StringBuilder();

            if (!string.IsNullOrEmpty(command.Connection.Database))
                sql.AppendLine("USE " + command.Connection.Database + ";");

            sql.Append(command.CommandText);

            foreach (IDataParameter parameter in command.Parameters)
            {
                sql.Replace(parameter.ParameterName, ParameterValue(parameter));
            }

            return sql.ToString();
        }

        static QueryDebugger()
        {
            //Store assembly to exclude API calls
            Assembly = Assembly.GetAssembly(typeof(QueryDebugger));
        }

        static readonly Assembly Assembly;

        /// <summary>
        /// Stores the exact executed query
        /// </summary>
        public string DebuggerQuery { get; private set; }
        /// <summary>
        /// Provieds a Instant to use SQL query that Contains all Variables and querys
        /// </summary>
        public string SqlQuery { get; private set; }

        /// <summary>
        /// Blocking if Stack Trace is not created
        /// </summary>
        public string StackTracer
        {
            get
            {
                if (_loaded)
                {
                    return _stackTracer;
                }
                _wokerTask.Wait();
                return _stackTracer;
            }
            private set
            {
                _stackTracer = value;
            }
        }

        public const string StartValuePart = @" {{";
        public const string EndValuePart = @"}} ";

        private Task _wokerTask;
        private bool _loaded;
        private string _stackTracer;

        private void Init()
        {
            var frames = new StackTrace().GetFrames();
            _wokerTask = new Task(() =>
            {
                try
                {
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
                }
                finally
                {
                    _loaded = true;
                }
            });

            _wokerTask.Start();
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
                case DbType.String:
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


    }
}
