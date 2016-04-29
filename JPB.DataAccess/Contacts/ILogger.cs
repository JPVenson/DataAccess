﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	/// Defines mehtods for Logging
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Writes one or more chars to the output by using string.Format
		/// </summary>
		/// <param name="content"></param>
		/// <param name="arguments"></param>
		void Write(string content, params object[] arguments);

		/// <summary>
		/// Writes one or more chars to the output by using string.Format followed by an Enviroment.NewLine
		/// </summary>
		/// <param name="content"></param>
		/// <param name="arguments"></param>
		void WriteLine(string content = null, params object[] arguments);
	}
}
