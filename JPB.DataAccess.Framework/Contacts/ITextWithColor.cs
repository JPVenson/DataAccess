namespace JPB.DataAccess.Framework.Contacts
{
	/// <summary>
	///		Combines a Text(string) with the given color
	/// </summary>
	/// <typeparam name="TColor"></typeparam>
	public interface ITextWithColor<out TColor>
	{
		/// <summary>
		///		The Assosiated Color
		/// </summary>
		TColor Color { get; }

		/// <summary>
		///		The Text fragment
		/// </summary>
		string Text { get; }
	}
}