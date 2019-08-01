using JPB.DataAccess.Framework.Contacts;

namespace JPB.DataAccess.Framework.Query.QueryItems.Conditional
{
	internal class ExpressionValue
	{
		public ExpressionValue(string queryValue, params IQueryParameter[] queryParameters)
		{
			QueryValue = queryValue;
			QueryParameters = queryParameters;
		}

		public string QueryValue { get; private set; }
		public IQueryParameter[] QueryParameters { get; private set; }

		//  User-defined conversion from double to Digit
		public static implicit operator ExpressionValue(string d)
		{
			return new ExpressionValue(d);
		}
	}
}