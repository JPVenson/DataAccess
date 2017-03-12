/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.DebuggerHelper
{
    /// <summary>
    /// Generates a easy debuggable object that contains all infos about the Generated Query.
    /// <b>Hint</b>: This class consumes a lot of time to load. Disable it in Production.
    /// </summary>
    [DebuggerDisplay("QueryCommand : {DebuggerQuery}", Name = "QueryCommand")]
    public class QueryDebugger : IDisposable
    {
        private readonly IDbCommand _command;
        private readonly IDatabase _source;

        /// <summary>
        /// The start value part
        /// </summary>
        public const string StartValuePart = @" {{";
        /// <summary>
        /// The end value part
        /// </summary>
        public const string EndValuePart = @"}} ";
        private static readonly Assembly Assembly;
        private bool _loaded;
        private string _stackTracer;
        private Task _wokerTask;
        private CancellationTokenSource _cancellationToken;

        static QueryDebugger()
        {
            //Store assembly to exclude API calls
            Assembly = Assembly.GetAssembly(typeof(QueryDebugger));
        }

        /// <summary>
        ///     Creates a Debugger that contains some debugging data
        /// </summary>
        internal QueryDebugger(IDbCommand command, IDatabase source)
        {
            _command = command;
            _source = source;
            Init();
            //Init async because this could be time consuming
            _loaded = false;
            if (UseDefaultDatabase != null && _source == null)
            {
                _source = new DefaultDatabaseAccess();
                _source.Attach(UseDefaultDatabase);
            }
        }

        /// <summary>
        ///     When set to true the QueryCommand debugger creates an own instance the the Default database connection assumd by the type
        ///     of the IDbCommand it contains
        /// </summary>
        public static IDatabaseStrategy UseDefaultDatabase { get; set; }

        /// <summary>
        ///     Stores the exact executed query
        /// </summary>
        public string DebuggerQuery { get; private set; }

        /// <summary>
        ///     Provieds a Instant to use SQL query that Contains all Variables and querys
        /// </summary>
        public string SqlQuery { get; private set; }

        /// <summary>
        ///     Blocking if Stack Trace is not created
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
            private set { _stackTracer = value; }
        }

        private void Init()
        {
            var frames = new StackTrace().GetFrames();
            _cancellationToken = new CancellationTokenSource();
            //This call is a bit of work so kick it off to a Task and let it run
            //we have to do it here because inside of the task this info is lost
            _wokerTask = new Task(stack =>
            {
                try
                {
                    IEnumerable<StackFrame> stackFrames;
                    if (stack != null)
                    {
                        stackFrames = (stack as IEnumerable<StackFrame>).Where(s =>
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


                Refresh();
            }, frames, _cancellationToken.Token, TaskCreationOptions.PreferFairness);
            _wokerTask.Start();
        }

        private string GenericCommandToQuery(IDbCommand command)
        {
            var sql = new StringBuilder();

            if (!string.IsNullOrEmpty(command.Connection.Database))
                sql.AppendLine("USE [" + command.Connection.Database + "];");

            foreach (IDataParameter parameter in command.Parameters)
            {
                sql.AppendFormat("DECLARE {0} {1} = {2}",
                    parameter.ParameterName,
                    parameter.DbType.ToString().ToUpper(),
                    ParameterValue(parameter));
                sql.AppendLine();
            }
            sql.Append(command.CommandText);

            return sql.ToString();
        }


        /// <summary>
        /// Formats the sp to string by using the DbType
        /// </summary>
        /// <param name="sp">The sp.</param>
        /// <returns></returns>
        public static String ParameterValue(IDataParameter sp)
        {
            var retval = "";

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
                case DbType.Guid:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                    break;

                case DbType.Boolean:
                    retval = (sp.Value is bool && (bool)sp.Value) ? "1" : "0";
                    break;

                case DbType.Binary:
                case DbType.Byte:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.StringFixedLength:
                    return sp.Value.ToString();
                default:
                    retval = sp.Value.ToString().Replace("'", "''");
                    break;
            }

            return retval;
        }

        internal void Render(StringBuilderInterlaced obj)
        {
            obj.Up()
                .AppendInterlacedLine("{")
                    .Up()
                    .AppendInterlacedLine("DebuggerQuery = \"{0}\"", DebuggerQuery)
                    .AppendInterlacedLine("SqlQuery = {0}", SqlQuery)
                    .Down()
                .AppendInterlacedLine("}");
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilderInterlaced();
            this.Render(sb);
            return sb.ToString();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _cancellationToken.Cancel();
            this.DebuggerQuery = null;
            this._wokerTask.Dispose();
        }

        /// <summary>
        /// Refreshes all internal Members
        /// </summary>
        public void Refresh()
        {
            var debugquery = new StringBuilder(this._command.CommandText);
            var formartCommandToQuery = _source != null
                ? _source.FormartCommandToQuery(this._command)
                : GenericCommandToQuery(this._command);

            DebuggerQuery = debugquery.ToString();
            SqlQuery = formartCommandToQuery;
        }
    }
}