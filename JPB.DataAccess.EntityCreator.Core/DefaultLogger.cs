using System;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.EntityCreator.Core
{
	internal class DefaultLogger : ILogger
	{
		/// <summary>
		/// Writes one or more chars to the output by using string.Format
		/// </summary>
		/// <param name="content"></param>
		/// <param name="arguments"></param>
		public void Write(string content, params object[] arguments)
		{
			Console.Write(content, arguments);
		}

		/// <summary>
		/// Writes one or more chars to the output by using string.Format followed by an Enviroment.NewLine
		/// </summary>
		/// <param name="content"></param>
		/// <param name="arguments"></param>
		public void WriteLine(string content = null, params object[] arguments)
		{
			Console.WriteLine(content, arguments);
		}
	}
}