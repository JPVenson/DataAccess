using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.DebuggerHelper
{
    [DebuggerDisplay("Query : {DebuggerQuery}", Name = "Query")]
    public class QueryDebugger
    {
        public string DebuggerQuery { get; private set; }
        public string SqlQuery { get; private set; }
        
        public const string StartValuePart = @" {{";
        public const string EndValuePart = @"}} ";
        
        public QueryDebugger(IDbCommand command)
        {
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
