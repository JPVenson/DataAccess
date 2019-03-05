#region

using System;
using System.Diagnostics;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	[DebuggerDisplay("{" + nameof(MethodName) + "}")]
	[Serializable]
	internal sealed class PropertyHelper<TAtt> : MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		private dynamic _getter;
		private dynamic _setter;

		public PropertyHelper(MethodBase accessorMethod) : base(accessorMethod)
		{
			MethodInfo = accessorMethod;
			MethodName = accessorMethod.Name;
		}

		public void SetGet(dynamic getter)
		{
			_getter = getter;
		}

		public void SetSet(dynamic setter)
		{
			_setter = setter;
		}

		public override object Invoke(dynamic target, params dynamic[] param)
		{
			if (_getter != null)
			{
				return _getter(target);
			}
			var paramOne = param[0];
			var result = _setter(target, paramOne);
			return result;
		}
	}
}