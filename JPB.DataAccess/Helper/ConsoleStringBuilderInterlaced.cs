using System;

namespace JPB.DataAccess.Helper
{
	/// <inheritdoc />
	public class ConsoleStringBuilderInterlaced : StringBuilderInterlaced<ConsoleColorWrapper>
	{
		/// <summary>
		///     Writes to console.
		/// </summary>
		public virtual void WriteToConsole()
		{
			WriteToSteam(Console.Out, color => Console.ForegroundColor = color,
			() => Console.ForegroundColor = ConsoleColor.White);
		}
	}
}