using System.Diagnostics;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Logger
{
	/// <summary>
	///     The default logger that is used if no other is specified
	/// </summary>
	public class DefaultLogger : ILogger
	{
		public void Write(string content, params object[] arguments)
		{
			Trace.Write(string.Format(content, arguments));
		}

		public void WriteLine(string content, params object[] arguments)
		{
			Trace.WriteLine(string.Format(content, arguments));
		}
	}
}