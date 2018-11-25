#region

using System;
using System.Diagnostics;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	[DebuggerDisplay("{MethodName}")]
	[Serializable]
	internal sealed class PropertyHelper<TAtt> : MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		private Delegate _getter;
		private Delegate _setter;

		public PropertyHelper(MethodBase accessorMethod) : base(accessorMethod)
		{
			MethodInfo = accessorMethod;
			MethodName = accessorMethod.Name;
		}

		public void SetGet(Delegate getter)
		{
			_getter = getter;
		}

		public void SetSet(Delegate setter)
		{
			_setter = setter;
		}

		public override object Invoke(object target, params object[] param)
		{
			if (_getter != null)
			{
				return _getter.DynamicInvoke(target);
			}
			var paramOne = param[0];
			var result = _setter.DynamicInvoke(target, paramOne);
			return result;
		}
	}
}