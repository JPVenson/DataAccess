using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Sets an type to be associated with the current class.
	///     TO BE SUPPORTED
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class MethodProxyAttribute : DataAccessAttribute
	{
		readonly Type _methodProxy;

		/// <summary>
		///     Allows to create a proxy class that should contains Factory methods for the current class
		/// </summary>
		/// <param name="methodProxy"></param>
		public MethodProxyAttribute(Type methodProxy)
		{
			_methodProxy = methodProxy;
		}

		/// <summary>
		///     The assocaiated type
		/// </summary>
		public Type MethodProxy
		{
			get { return _methodProxy; }
		}
	}
}