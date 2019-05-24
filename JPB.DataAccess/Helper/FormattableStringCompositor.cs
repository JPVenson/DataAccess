using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///		Composes a interpolated string into an QueryString that has escaped chars
	/// </summary>
	public class FormattableStringCompositor
	{
		/// <summary>
		///		The enumeration of all query Parameters
		/// </summary>
		public IQueryParameter[] QueryParameters { get; private set; }

		/// <summary>
		///		The processed query
		/// </summary>
		public string Query { get; private set; }

		private FormattableStringCompositor()
		{
			
		}

		private static Regex FormatExtractorRegex = new Regex(@"{+(?:(\d+)(?:(,?\d*)?|:(\w|\d|\s)+))}+", RegexOptions.Compiled);

		///  <summary>
		/// 		Converts the FormattableString into a Compositor
		///  </summary>
		///  <param name="str"></param>
		///  <returns></returns>
		public static FormattableStringCompositor Factory(FormattableString str)
		{
			var composedString = new FormattableStringCompositor();
			var objects = str.GetArguments();
			var format = str.Format;
			var matches = FormatExtractorRegex.Matches(format);
			var args = new List<IQueryParameter>();

			foreach (Match match in matches)
			{
				if (match.Value.StartsWith("{{"))
				{
					//ignore escaped
					continue;
				}
				var index = match.Groups[1].Value;
				var pad = match.Groups[2].Value;
				var formatArgument = match.Groups[3].Value;
				var trimIndex = index.Trim('{', '}');

				var argName = $"@arg_{trimIndex}";

				if (args.Any(e => e.Name.Equals(argName)))
				{
					format = format.Replace(match.Value, argName);
					continue;
				}

				
				var intIndex = int.Parse(trimIndex);
				if (intIndex > objects.Length)
				{
					throw new IndexOutOfRangeException($"The index {intIndex} for the format of '{format}' " +
					                                   $"was out of range. " +
					                                   $"Count of provided arguments was {objects.Length}");
				}


				if (!string.IsNullOrWhiteSpace(pad) || !string.IsNullOrWhiteSpace(formatArgument))
				{
					args.Add(new QueryParameter(argName, string.Format(match.Value, objects)));
					format = format.Replace(match.Value, argName);
				}
				else
				{
					var value = objects[intIndex];
					args.Add(new QueryParameter(argName, value));
					format = format.Replace(match.Value, argName);
				}
			}

			composedString.Query = format;
			composedString.QueryParameters = args.ToArray();

			return composedString;
		}
	}
}
