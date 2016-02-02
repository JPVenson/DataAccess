using System;
using System.Data;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///     Example Implimentation of IQueryParameter
	/// </summary>
	public class QueryParameter : IQueryParameter
	{
		private object m_value;
		private Type m_sourceType;
		// ReSharper disable CSharpWarnings::CS1591
		private QueryParameter()
		{
		}

		public QueryParameter(string name, object value)
		{
			Name = name;
			Value = value;
			SourceType = value.GetType();
		}

		public QueryParameter(string name, object value, Type valType)
		{
			Name = name;
			Value = value;
			SourceType = valType;
		}

		public QueryParameter(string name, object value, DbType valType)
		{
			Name = name;
			Value = value;
			SourceDbType = valType;
		}

		#region IQueryParameter Members

		public string Name { get; set; }

		public object Value
		{
			get { return m_value; }
			set
			{
				SourceType = value == null ? DBNull.Value.GetType() : value.GetType();
				m_value = value;
			}
		}

		public Type SourceType
		{
			get { return m_sourceType; }
			set
			{
				m_sourceType = value;
				SourceDbType = DbAccessLayer.Map(value).Value;
			}
		}

		public DbType SourceDbType { get; set; }

		#endregion

		// ReSharper restore CSharpWarnings::CS1591

		/// <summary>
		///     Renders the current object
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
				.AppendIntedLine("SourceType = {0}", SourceType.ToString())
				.AppendIntedLine("SourceDbType = {0}", SourceDbType)
				.Down()
				.AppendInted("}");
		}

		public override string ToString()
		{
			return Render();
		}
	}
}