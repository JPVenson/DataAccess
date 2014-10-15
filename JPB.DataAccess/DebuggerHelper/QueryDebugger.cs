using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.DebuggerHelper
{
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
            if(enumerable.Any())
                StackTracer = enumerable.Aggregate((e, f) => e + Environment.NewLine + f);
            var debugquery = new StringBuilder(command.CommandText);
            var sqlReady = new StringBuilder(command.CommandText);

            foreach (var parameter in command.Parameters.Cast<IDataParameter>())
            {
                string param;

                if (parameter.Value is string)
                {
                    param = "'" + parameter.Value + "'";
                }
                else
                {
                    param = parameter.Value.ToString();
                }

                debugquery.Replace(parameter.ParameterName, StartValuePart + param + EndValuePart);
                sqlReady.Replace(parameter.ParameterName, param);
            }

            DebuggerQuery = debugquery.ToString();
            SqlQuery = sqlReady.ToString();
        }
    }
}
