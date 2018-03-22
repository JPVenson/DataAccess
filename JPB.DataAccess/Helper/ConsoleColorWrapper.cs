using System;

namespace JPB.DataAccess.Helper
{
	/// <inheritdoc />
	public class ConsoleColorWrapper
	{
		/// <inheritdoc />
		public ConsoleColor Value { get; private set; }

		/// <inheritdoc />
		public static implicit operator ConsoleColor(ConsoleColorWrapper wrapper)
		{
			return wrapper.Value;
		}

		/// <inheritdoc />
		public static implicit operator ConsoleColorWrapper(ConsoleColor wrapper)
		{
			return new ConsoleColorWrapper() { Value = wrapper };
		}
	}
}