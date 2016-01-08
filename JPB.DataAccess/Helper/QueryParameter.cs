namespace JPB.DataAccess.Helper
{
	/// <summary>
	/// Example Implimentation of IQueryParameter
	/// </summary>
	public class QueryParameter : IQueryParameter
	{
		// ReSharper disable CSharpWarnings::CS1591
		public QueryParameter()
		{
		}

		public QueryParameter(string name, object value)
		{
			Name = name;
			Value = value;
		}

		#region IQueryParameter Members

		/// <summary>
		///     Name with @ or without it
		///     if the system detects a name without @ it will add it
		/// </summary>
		public string Name { get; set; }

		public object Value { get; set; }

		#endregion
		// ReSharper restore CSharpWarnings::CS1591

		/// <summary>
		/// Renders the current object
		/// </summary>
		/// <returns></returns>
		public string Render()
		{
			var sb = new StringBuilderIntend();
			Render(sb);
			return sb.ToString();
		}

		internal void Render(StringBuilderIntend sb)
		{
			var value = "{Null}";
			if (Value != null)
			{
				value = Value.ToString();
			}
			sb.AppendIntedLine("neq QueryParameter {")
				.Up()
				.AppendIntedLine("Name = {0},", Name)
				.AppendIntedLine("Value.ToString = {0}", value)
				.Down()
				.AppendInted("}");
		}

		public override string ToString()
		{
			return Render();
		}
	}
}