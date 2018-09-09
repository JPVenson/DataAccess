using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace JPB.DataAccess.Tests.Overwrite.Framework.MySql
{
	[DebuggerDisplay("{" + nameof(DebugMessage) + "()}")]
	public class MySqlLogline
	{
		public string DebugMessage()
		{
			if (Time == default(DateTime))
			{
				return Orginal;
			}

			return $"{LogLevel} {Message}";
		}

		public static Regex ParseLogLineRegEx = new Regex(@"([^\s]*)\s([0-9])*\s(\[[a-zA-Z]+\])\s(\[[^\]]+\])\s(\[[^\]]+\])\s(.*)", RegexOptions.Multiline);

		public class ParseResult
		{
			public ParseResult(int firstCharConsumed, int lastCharConsumed, MySqlLogline[] loglines)
			{
				FirstCharConsumed = firstCharConsumed;
				LastCharConsumed = lastCharConsumed;
				Loglines = loglines;
			}

			public int FirstCharConsumed { get; private set; }
			public int LastCharConsumed { get; private set; }
			public MySqlLogline[] Loglines { get; private set; }
		}

		public static ParseResult ParseLogLine(string logLines)
		{
			var result = new List<MySqlLogline>();
			var lastCharConsumed = 0;
			var firstCharConsumed = 0;
			foreach (Match match in ParseLogLineRegEx.Matches(logLines))
			{
				if (firstCharConsumed == 0)
				{
					firstCharConsumed = match.Index;
				}

				lastCharConsumed = match.Index + match.Length;
				result.Add(new MySqlLogline()
				{
					Orginal = match.Groups[0].Value,
					Time = DateTime.Parse(match.Groups[1].Value),
					Code = int.Parse(match.Groups[2].Value),
					LogLevel = match.Groups[3].Value,
					Instance = match.Groups[4].Value,
					Origin = match.Groups[5].Value,
					Message = match.Groups[6].Value,
				});
			}
			return new ParseResult(firstCharConsumed,lastCharConsumed, result.ToArray());
		}

		public DateTime Time { get; set; }
		public int Code { get; set; }
		public string LogLevel { get; set; }
		public string Instance { get; set; }
		public string Origin { get; set; }
		public string Message { get; set; }

		public string Orginal { get; set; }
	}
}