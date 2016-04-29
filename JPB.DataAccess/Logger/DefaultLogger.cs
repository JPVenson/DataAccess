using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Logger
{
	/// <summary>
	/// The default logger that is used if no other is specified
	/// </summary>
	public class DefaultLogger : ILogger
	{
		public void Write(string content, params object[] arguments)
		{
			if (content == null)
				content = string.Empty;
			Trace.Write(string.Format(content, arguments));
		}

		public void WriteLine(string content, params object[] arguments)
		{
			if (content == null)
				content = string.Empty;
			Trace.WriteLine(string.Format(content, arguments));
		}
	}
}
